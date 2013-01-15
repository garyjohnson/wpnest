using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		private ISessionProvider _sessionProvider;
		private IAnalyticsService _analyticsService;

		public NestWebService() {
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
		}

		public async Task<WebServiceResult> LoginAsync(string userName, string password) {
			WebRequest request = GetPostFormRequest("https://home.nest.com/user/login");
			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);
			Exception exception;

			try {
				WebResponse response = await request.GetResponseAsync();
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
				WebResponse response = await request.GetResponseAsync();
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

			WebRequest request = GetPostJsonRequest(url);
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

		public async Task<WebServiceResult> SetAwayModeAsync(Structure structure, bool isAway) {
			string url = string.Format(@"{0}/v2/put/structure.{1}", _sessionProvider.TransportUrl, structure.ID);
			var timestamp = GetCurrentTimeAsEpoch();
			string requestString = string.Format("{{\"away_timestamp\":{0},\"away\":{1},\"away_setter\":0}}", timestamp, isAway);

			return await SendPutRequestAsync(url, requestString);
		}

		private static double GetCurrentTimeAsEpoch() {
			var unixTime = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			double timestamp = Math.Floor(unixTime.TotalSeconds);
			return timestamp;
		}

		public async Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			string fanModeString = GetFanModeString(fanMode);
			string requestString = string.Format("{{\"fan_mode\":\"{0}\"}}", fanModeString);
			return await SendPutRequestAsync(url, requestString);
		}

		public async Task<WebServiceResult> SetHvacModeAsync(Thermostat thermostat, HvacMode hvacMode) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			string hvacModeString = GetHvacModeString(hvacMode);
			string requestString = string.Format("{{\"target_temperature_type\":\"{0}\"}}", hvacModeString);
			return await SendPutRequestAsync(url, requestString);
		}

		private string GetHvacModeString(HvacMode hvacMode) {
			if (hvacMode == HvacMode.Off)
				return "off";
			if (hvacMode == HvacMode.HeatOnly)
				return "heat";
			if (hvacMode == HvacMode.CoolOnly)
				return "cool";
			if (hvacMode == HvacMode.HeatAndCool)
				return "range";

			throw new InvalidOperationException();
		}

		private string GetFanModeString(FanMode fanMode) {
			if (fanMode == FanMode.Auto)
				return "auto";
			if(fanMode == FanMode.Off)
				return "off";

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

			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);

			string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\"}}]}}", thermostat.ID);
			await request.SetRequestStringAsync(requestString);
			Exception exception;

			try {
				WebResponse response = await request.GetResponseAsync();
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
			else if(IsNotFoundError(exception))
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
			double temperature = Math.Round(temperatureCelcius.CelciusToFahrenheit());
			double currentTemperatureCelcius = double.Parse(values["current_temperature"].Value<string>());
			double currentTemperature = Math.Round(currentTemperatureCelcius.CelciusToFahrenheit());
			bool isHeating = values["hvac_heater_state"].Value<bool>();
			bool isCooling = values["hvac_ac_state"].Value<bool>();
			return new GetThermostatStatusResult(temperature, currentTemperature, isHeating, isCooling);
		}

		private GetStatusResult ParseGetStatusResult(string responseString, string userId) {
			var structureResults = new List<Structure>();

			var values = JObject.Parse(responseString);
			var structures = values["user"][userId]["structures"];
			foreach (var structure in structures) {
				string structureId = structure.Value<string>().Replace("structure.", "");
				structureResults.Add(new Structure(structureId));
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
