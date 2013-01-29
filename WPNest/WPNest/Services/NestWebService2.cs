using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WPNest.Services {
	public class NestWebService2 : INestWebService2 {

		private ISessionProvider _sessionProvider;
		private IAnalyticsService _analyticsService;

		public NestWebService2() {
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
		}

		public void BeginLogin(string userName, string password, Action<WebServiceResult> completedCallback) {
			WebRequest request = GetPostFormRequest("https://home.nest.com/user/login");

			var state = new Dictionary<string, object> {
				            {"request", request},
				            {"userName", userName},
				            {"password", password},
							{"callback", completedCallback}};

			var requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			request.SetRequestStringAndThenExecute(requestString, state, (webRequest, userState) => {
				webRequest.BeginGetResponse(ContinueLoginAfterGetResponse, userState);
			});
		}

		private void ContinueLoginAfterGetResponse(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var callback = (Action<WebServiceResult>)state["callback"];

			WebServiceResult result;
			try {
				WebResponse response = request.EndGetResponse(asyncResult);
				string responseString = response.GetResponseString();
				CacheSession(responseString);
				result = new WebServiceResult();
			}
			catch (Exception exception) {
				var error = ParseWebServiceError(exception);
				result = new WebServiceResult(error, exception);
			}

			if (callback != null)
				callback(result);
		}

		public void BeginGetStatus(Action<GetStatusResult> completedCallback) {
			if (_sessionProvider.IsSessionExpired) {
				if (completedCallback != null)
					completedCallback(new GetStatusResult(WebServiceError.SessionTokenExpired, new SessionExpiredException()));
				return;
			}

			string url = string.Format("{0}/v2/mobile/user.{1}", _sessionProvider.TransportUrl, _sessionProvider.UserId);
			var request = GetGetRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);

			var state = new Dictionary<string, object> {
				            {"request", request},
							{"callback", completedCallback}};

			request.BeginGetResponse(ContinueGetStatusAfterGetResponse, state);
		}

		private void ContinueGetStatusAfterGetResponse(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var callback = (Action<WebServiceResult>)state["callback"];

			GetStatusResult result;
			try {
				WebResponse response = request.EndGetResponse(asyncResult);
				string responseString = response.GetResponseString();
				result = ParseGetStatusResult(responseString, _sessionProvider.UserId);
			}
			catch (Exception exception) {
				var error = ParseWebServiceError(exception);
				result = new GetStatusResult(error, exception);
			}

			callback(result);
		}

		public void BeginSetFanMode(Thermostat thermostat, FanMode fanMode, Action<WebServiceResult> completedCallback) {
			string url = string.Format(@"{0}/v2/put/device.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			string fanModeString = GetFanModeString(fanMode);
			string requestString = string.Format("{{\"fan_mode\":\"{0}\"}}", fanModeString);
			BeginSendPutRequest(url, requestString, completedCallback);
		}

		public void BeginChangeTemperature(Thermostat thermostat, double desiredTemperature, Action<WebServiceResult> completedCallback) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			double desiredTempCelcius = desiredTemperature.FahrenheitToCelcius();
			string requestString = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTempCelcius.ToString());
			BeginSendPutRequest(url, requestString, completedCallback);
		}

		public void BeginSendPutRequest(string url, string requestJson, Action<WebServiceResult> completedCallback) {
			if (_sessionProvider.IsSessionExpired) {
				if (completedCallback != null)
					completedCallback(new WebServiceResult(WebServiceError.SessionTokenExpired, new SessionExpiredException()));
				return;
			}

			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);

			var state = new Dictionary<string, object> {
				            {"request", request},
				            {"requestJson", requestJson},
							{"callback", completedCallback}};

			request.BeginGetRequestStream(ContinuePutRequestAfterGetRequestStream, state);

		}

		private void ContinuePutRequestAfterGetRequestStream(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var requestJson = (string)state["requestJson"];

			using (Stream stream = request.EndGetRequestStream(asyncResult)) {
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestJson);
				stream.Write(encodedRequestString, 0, encodedRequestString.Length);
			}

			request.BeginGetResponse(ContinuePutRequestAfterGetResponse, state);
		}

		private void ContinuePutRequestAfterGetResponse(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var completedCallback = (Action<WebServiceResult>)state["callback"];

			WebServiceResult result;
			try {
				request.EndGetResponse(asyncResult);
				result = new WebServiceResult();
			}
			catch (Exception exception) {
				var error = ParseWebServiceError(exception);
				result = new WebServiceResult(error, exception);
			}

			if (completedCallback != null)
				completedCallback(result);
		}

		public void BeginGetThermostatStatus(Thermostat thermostat, Action<GetThermostatStatusResult> completedCallback) {
			if (_sessionProvider.IsSessionExpired)
				completedCallback(new GetThermostatStatusResult(WebServiceError.SessionTokenExpired, new SessionExpiredException()));

			BeginGetSharedThermostatProperties(thermostat, (sharedJson, sharedError, sharedException) => {
				if (sharedException != null) {
					completedCallback(new GetThermostatStatusResult(sharedError, sharedException));
					return;
				}

				BeginGetDeviceThermostatProperties(thermostat, (deviceJson, deviceError, deviceException) => {
					if (deviceException != null) {
						completedCallback(new GetThermostatStatusResult(deviceError, deviceException));
						return;
					}

					var statusResult = ParseGetTemperatureResult(sharedJson);
					var values = JObject.Parse(deviceJson);
					statusResult.FanMode = GetFanModeFromString(values["fan_mode"].Value<string>());

					completedCallback(statusResult);
				});
			});
		}

		private void BeginGetDeviceThermostatProperties(Thermostat thermostat, Action<string, WebServiceError, Exception> onCompleted) {
			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);
			string requestString = string.Format("{{\"keys\":[{{\"key\":\"device.{0}\"}}]}}", thermostat.ID);

			var state = new Dictionary<string, object>
			            {
				            {"request", request},
				            {"onCompleted", onCompleted}
			            };

			request.SetRequestStringAndThenExecute(requestString, state, (webRequest, userState) => {
				webRequest.BeginGetResponse(ContinueGetThermostatPropertiesAfterGetResponse, userState);
			});
		}

		private void BeginGetSharedThermostatProperties(Thermostat thermostat, Action<string, WebServiceError, Exception> onCompleted) {
			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);
			string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\"}}]}}", thermostat.ID);

			var state = new Dictionary<string, object>
			            {
				            {"request", request},
				            {"onCompleted", onCompleted}
			            };

			request.SetRequestStringAndThenExecute(requestString, state, (webRequest, userState) => {
				webRequest.BeginGetResponse(ContinueGetThermostatPropertiesAfterGetResponse, userState);
			});
		}

		private void ContinueGetThermostatPropertiesAfterGetResponse(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var onCompleted = (Action<string, WebServiceError, Exception>)state["onCompleted"];

			WebServiceError error;
			Exception exception = null;
			string content = null;

			try {
				WebResponse response = request.EndGetResponse(asyncResult);
				content = response.GetResponseString();
			}
			catch (Exception ex) {
				exception = ex;
			}

			error = ParseWebServiceError(exception);
			onCompleted(content, error, exception);
		}

		public void BeginUpdateTransportUrl(Action<WebServiceResult> onCompleted) {
			WebRequest request = GetPostJsonRequest("https://home.nest.com/user/service_urls");
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);

			var state = new Dictionary<string, object> {
				            {"request", request},
				            {"onCompleted", onCompleted}
			            };

			request.BeginGetResponse(ContinueUpdateTransportUrlAfterGetResponse, state);
		}

		private void ContinueUpdateTransportUrlAfterGetResponse(IAsyncResult asyncResult) {
			var state = (Dictionary<string, object>)asyncResult.AsyncState;
			var request = (WebRequest)state["request"];
			var onCompleted = (Action<WebServiceResult>)state["onCompleted"];

			Exception exception = null;
			try {
				WebResponse response = request.EndGetResponse(asyncResult);
				string strContent = response.GetResponseString();
				var jsonResult = ParseAsJsonOrNull(strContent);
				var transportUrl = jsonResult["urls"]["transport_url"].Value<string>();
				_sessionProvider.UpdateTransportUrl(transportUrl);
				onCompleted(new WebServiceResult());
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = ParseWebServiceError(exception);
			onCompleted(new WebServiceResult(error, exception));
		}

		private static FanMode GetFanModeFromString(string fanMode) {
			if (fanMode == "auto")
				return FanMode.Auto;
			if (fanMode == "on")
				return FanMode.On;

			throw new InvalidOperationException();
		}

		private static string GetFanModeString(FanMode fanMode) {
			if (fanMode == FanMode.Auto)
				return "auto";
			if (fanMode == FanMode.On)
				return "on";

			throw new InvalidOperationException();
		}

		private WebServiceError ParseWebServiceError(Exception exception) {
			var error = WebServiceError.Unknown;
			if (IsInvalidCredentialsError(exception))
				error = WebServiceError.InvalidCredentials;
			else if (IsSessionTokenExpiredError(exception))
				error = WebServiceError.SessionTokenExpired;
			else if (IsNotFoundError(exception))
				error = WebServiceError.ServerNotFound;

			return error;
		}

		private bool IsNotFoundError(Exception exception) {
			HttpStatusCode? statusCode = GetHttpStatusCodeFromException(exception);
			return (statusCode.HasValue && statusCode.Value == HttpStatusCode.NotFound);
		}

		private bool IsSessionTokenExpiredError(Exception exception) {
			HttpStatusCode? statusCode = GetHttpStatusCodeFromException(exception);
			return (statusCode.HasValue && statusCode.Value == HttpStatusCode.Unauthorized);
		}

		private bool IsInvalidCredentialsError(Exception exception) {
			bool isInvalidCredentials = false;
			var webException = exception as WebException;
			if (webException != null && webException.Response != null) {
				string responseString = webException.Response.GetResponseString();
				var values = ParseAsJsonOrNull(responseString);
				if (values != null) {
					var errorJson = values["error"];
					if (errorJson != null) {
						var errorMessage = errorJson.Value<string>();
						isInvalidCredentials = errorMessage.Equals("access_denied");
					}
				}
			}

			return isInvalidCredentials;
		}

		private static JObject ParseAsJsonOrNull(string responseString) {
			JObject values = null;
			try {
				values = JObject.Parse(responseString);
			}
			catch (Newtonsoft.Json.JsonException) { }
			return values;
		}

		private static GetThermostatStatusResult ParseGetTemperatureResult(string strContent) {
			var values = JObject.Parse(strContent);
			double temperatureCelcius = double.Parse(values["target_temperature"].Value<string>());
			double currentTemperatureCelcius = double.Parse(values["current_temperature"].Value<string>());

			var result = new GetThermostatStatusResult();
			result.TargetTemperature = Math.Round(temperatureCelcius.CelciusToFahrenheit());
			result.CurrentTemperature = Math.Round(currentTemperatureCelcius.CelciusToFahrenheit());
			result.IsHeating = values["hvac_heater_state"].Value<bool>();
			result.IsCooling = values["hvac_ac_state"].Value<bool>();


			return result;
		}

		private GetStatusResult ParseGetStatusResult(string responseString, string userId) {
			var structureResults = new List<Structure>();

			var values = JObject.Parse(responseString);
			var structures = values["user"][userId]["structures"];
			foreach (var structure in structures) {
				string structureId = structure.Value<string>().Replace("structure.", "");
				var structureModel = new Structure(structureId);
				structureResults.Add(structureModel);
			}


			int deviceCount = 0;
			foreach (var structureResult in structureResults) {
				var devices = values["structure"][structureResult.ID]["devices"];
				foreach (var device in devices) {
					deviceCount++;
					string thermostatId = device.Value<string>().Replace("device.", "");
					var thermostat = new Thermostat(thermostatId);
					structureResult.Thermostats.Add(thermostat);
				}
			}

			foreach (var structureResult in structureResults) {
				foreach (var thermostat in structureResult.Thermostats) {
					var thermostatValues = values["shared"][thermostat.ID];
					double temperature = double.Parse(thermostatValues["target_temperature"].Value<string>());
					thermostat.TargetTemperature = Math.Round(temperature.CelciusToFahrenheit());
					double currentTemperature = double.Parse(thermostatValues["current_temperature"].Value<string>());
					thermostat.CurrentTemperature = Math.Round(currentTemperature.CelciusToFahrenheit());
					thermostat.IsHeating = thermostatValues["hvac_heater_state"].Value<bool>();
					thermostat.IsCooling = thermostatValues["hvac_ac_state"].Value<bool>();
					thermostatValues = values["device"][thermostat.ID];
					thermostat.FanMode = GetFanModeFromString(thermostatValues["fan_mode"].Value<string>());
				}
			}

			_analyticsService.LogEvent("Structures: {0}, Devices: {1}", structures.Count(), deviceCount);
			return new GetStatusResult(structureResults);
		}

		private static WebRequest GetGetRequest(string url) {
			WebRequest request = WebRequest.Create(new Uri(url));
			request.Method = "GET";
			return request;
		}

		private static WebRequest GetPostFormRequest(string url) {
			WebRequest request = WebRequest.Create(new Uri(url));
			request.ContentType = ContentType.Form;
			request.Method = "POST";
			return request;
		}

		private static WebRequest GetPostJsonRequest(string url) {
			WebRequest request = WebRequest.Create(new Uri(url));
			request.ContentType = ContentType.Json;
			request.Method = "POST";
			return request;
		}

		private void CacheSession(string responseString) {
			var values = JObject.Parse(responseString);
			var accessToken = values["access_token"].Value<string>();
			var accessTokenExpirationDate = values["expires_in"].Value<DateTime>();
			var userId = values["userid"].Value<string>();
			var transportUrl = values["urls"]["transport_url"].Value<string>();

			_sessionProvider.SetSession(transportUrl, userId, accessToken, accessTokenExpirationDate);
		}

		private string UrlEncode(string value) {
			return HttpUtility.UrlEncode(value);
		}

		private static void SetAuthorizationHeaderOnRequest(WebRequest request, string accessToken) {
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);
		}

		private static void SetNestHeadersOnRequest(WebRequest request, string userId) {
			request.Headers["X-nl-protocol-version"] = "1";
			request.Headers["X-nl-user-id"] = userId;
		}

		private static HttpStatusCode? GetHttpStatusCodeFromException(Exception exception) {
			var webException = exception as WebException;
			if (webException != null && webException.Response != null) {
				var response = (HttpWebResponse)webException.Response;
				return response.StatusCode;
			}
			return null;
		}

	}
}
