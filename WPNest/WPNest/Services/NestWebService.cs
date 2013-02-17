using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WPNest.Web;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		private ISessionProvider _sessionProvider;
		private IAnalyticsService _analyticsService;
		private IWebRequestProvider _webRequestProvider;

		public NestWebService() {
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
			_webRequestProvider = ServiceContainer.GetService<IWebRequestProvider>();
		}

		public async Task<WebServiceResult> LoginAsync(string userName, string password) {
			IWebRequest request = GetPostFormRequest("https://home.nest.com/user/login");
			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);
			Exception exception;

			try {
				IWebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				CacheSession(responseString);
				return new WebServiceResult();
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new WebServiceResult(error, exception);
		}

		public async Task<GetStatusResult> GetStatusAsync() {
			if (_sessionProvider.IsSessionExpired)
				return new GetStatusResult(WebServiceError.SessionTokenExpired, new SessionExpiredException());

			string url = string.Format("{0}/v2/mobile/user.{1}", _sessionProvider.TransportUrl, _sessionProvider.UserId);
			var request = GetGetRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			Exception exception = null;

			try {
				IWebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				return ParseGetStatusResult(responseString, _sessionProvider.UserId);
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new GetStatusResult(error, exception);
		}

		private async Task<WebServiceResult> SendPutRequestAsync(string url, string requestJson) {
			if (_sessionProvider.IsSessionExpired)
				return new GetStatusResult(WebServiceError.SessionTokenExpired, new SessionExpiredException());

			IWebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);

			await request.SetRequestStringAsync(requestJson);
			Exception exception = null;

			try {
				await request.GetResponseAsync();
				return new WebServiceResult();
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new WebServiceResult(error, exception);
		}

		public async Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode) {
			string url = string.Format(@"{0}/v2/put/device.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			string fanModeString = GetFanModeString(fanMode);
			string requestString = string.Format("{{\"fan_mode\":\"{0}\"}}", fanModeString);
			return await SendPutRequestAsync(url, requestString);
		}

		public async Task<WebServiceResult> UpdateTransportUrlAsync() {
			IWebRequest request = GetPostJsonRequest("https://home.nest.com/user/service_urls");
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);

			Exception exception = null;
			try {
				IWebResponse response = await request.GetResponseAsync();
				string strContent = await response.GetResponseStringAsync();
				var jsonResult = ParseAsJsonOrNull(strContent);
				var transportUrl = jsonResult["urls"]["transport_url"].Value<string>();
				_sessionProvider.UpdateTransportUrl(transportUrl);
				return new WebServiceResult();
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new WebServiceResult(error, exception);
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

		public async Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			double desiredTempCelcius = desiredTemperature.FahrenheitToCelcius();
			string requestString = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTempCelcius.ToString());
			return await SendPutRequestAsync(url, requestString);
		}

		public async Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat) {
			if (_sessionProvider.IsSessionExpired)
				return new GetThermostatStatusResult(WebServiceError.SessionTokenExpired, new SessionExpiredException());

			GetThermostatStatusResult result = await GetSharedThermostatPropertiesAsync(thermostat);
			if (result.Exception == null) {
				result = await GetDeviceThermostatPropertiesAsync(thermostat, result);
			}

			return result;
		}

		private async Task<GetThermostatStatusResult> GetDeviceThermostatPropertiesAsync(Thermostat thermostat, GetThermostatStatusResult result) {
			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			IWebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);
			string requestString = string.Format("{{\"keys\":[{{\"key\":\"device.{0}\"}}]}}", thermostat.ID);
			await request.SetRequestStringAsync(requestString);

			Exception exception;
			try {
				IWebResponse response = await request.GetResponseAsync();
				string strContent = await response.GetResponseStringAsync();
				var values = JObject.Parse(strContent);
				result.FanMode = GetFanModeFromString(values["fan_mode"].Value<string>());
				return result;
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new GetThermostatStatusResult(error, exception);
		}

		private async Task<GetThermostatStatusResult> GetSharedThermostatPropertiesAsync(Thermostat thermostat) {
			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			IWebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);
			string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\"}}]}}", thermostat.ID);
			await request.SetRequestStringAsync(requestString);

			Exception exception;
			try {
				IWebResponse response = await request.GetResponseAsync();
				string strContent = await response.GetResponseStringAsync();
				return ParseGetTemperatureResult(strContent);
			}
			catch (Exception ex) {
				exception = ex;
			}

			var error = await ParseWebServiceErrorAsync(exception);
			return new GetThermostatStatusResult(error, exception);
		}

		private async Task<WebServiceError> ParseWebServiceErrorAsync(Exception exception) {
			var error = WebServiceError.Unknown;

			if (await IsInvalidCredentialsErrorAsync(exception))
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

		private async Task<bool> IsInvalidCredentialsErrorAsync(Exception exception) {
			bool isInvalidCredentials = false;
			var webException = exception as WebException;
			if (webException != null && webException.Response != null) {
				string responseString = await webException.Response.GetResponseStringAsync();
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
				var structure = values["structure"][structureResult.ID];
				var devices = structure["devices"];
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

		private IWebRequest GetGetRequest(string url) {
			IWebRequest request = _webRequestProvider.CreateRequest(new Uri(url));
			request.Method = "GET";
			return request;
		}

		private IWebRequest GetPostFormRequest(string url) {
			IWebRequest request = _webRequestProvider.CreateRequest(new Uri(url));
			request.ContentType = ContentType.Form;
			request.Method = "POST";
			return request;
		}

		private IWebRequest GetPostJsonRequest(string url) {
			IWebRequest request = _webRequestProvider.CreateRequest(new Uri(url));
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

		private static void SetAuthorizationHeaderOnRequest(IWebRequest request, string accessToken) {
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);
		}

		private static void SetNestHeadersOnRequest(IWebRequest request, string userId) {
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
