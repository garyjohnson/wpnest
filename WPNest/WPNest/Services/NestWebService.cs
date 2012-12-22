﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpGIS;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		private ISessionProvider _sessionProvider;

		public NestWebService() {
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
		}

		public async Task<WebServiceResult> LoginAsync(string userName, string password) {
			WebRequest request = GetPostFormRequest("https://home.nest.com/user/login");

			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);

			Exception responseException = null;
			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				CacheSession(responseString);
			}
			catch (Exception exception) {
				responseException = exception;
			}

			if (responseException != null) {
				var error = WebServiceError.Unknown;
				var webException = responseException as WebException;
				if(webException != null && webException.Response != null) {
					string responseString = await webException.Response.GetResponseStringAsync();
					var values = JObject.Parse(responseString);
					var structures = values["error"];
					string errorMessage = structures.Value<string>();
					if(errorMessage.Equals("access_denied"))
						error = WebServiceError.InvalidCredentials;
				}

				return new WebServiceResult(error, responseException);
			}
			else {
				return new WebServiceResult();
			}
		}

		public async Task<GetStatusResult> GetStatusAsync() {
			if (_sessionProvider.IsSessionExpired)
				return new GetStatusResult(new SessionExpiredException());

			string url = string.Format("{0}/v2/mobile/user.{1}", _sessionProvider.TransportUrl, _sessionProvider.UserId);
			var request = GetGetRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);

			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				return ParseGetStatusResult(responseString, _sessionProvider.UserId);
			}
			catch (Exception exception) {
				return new GetStatusResult(exception);
			}
		}

		public async Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature) {
			if (_sessionProvider.IsSessionExpired)
				return new GetStatusResult(new SessionExpiredException());

			string url = string.Format(@"{0}/v2/put/shared.{1}", _sessionProvider.TransportUrl, thermostat.ID);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);

			double desiredTempCelcius = desiredTemperature.FahrenheitToCelcius();
			string requestString = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTempCelcius.ToString());
			await request.SetRequestStringAsync(requestString);

			try {
				await request.GetResponseAsync();
				return new WebServiceResult();
			}
			catch (Exception exception) {
				return new WebServiceResult(exception);
			}
		}

		public async Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat) {
			if (_sessionProvider.IsSessionExpired)
				return new GetThermostatStatusResult(new SessionExpiredException());

			string url = string.Format("{0}/v2/subscribe", _sessionProvider.TransportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, _sessionProvider.AccessToken);
			SetNestHeadersOnRequest(request, _sessionProvider.UserId);

			string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\"}}]}}", thermostat.ID);
			await request.SetRequestStringAsync(requestString);

			try {
				WebResponse response = await request.GetResponseAsync();
				string strContent = await response.GetResponseStringAsync();
				return ParseGetTemperatureResult(strContent);
			}
			catch (Exception exception) {
				return new GetThermostatStatusResult(exception);
			}
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

		private static GetStatusResult ParseGetStatusResult(string responseString, string userId) {
			var structureResults = new List<Structure>();

			var values = JObject.Parse(responseString);
			var structures = values["user"][userId]["structures"];
			foreach (var structure in structures) {
				string structureId = structure.Value<string>().Replace("structure.", "");
				structureResults.Add(new Structure(structureId));
			}

			foreach (var structureResult in structureResults) {
				var devices = values["structure"][structureResult.ID]["devices"];
				foreach (var device in devices) {
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

			return new GetStatusResult(structureResults);
		}

		private static WebRequest GetGetRequest(string url) {
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.Method = "GET";
			return request;
		}

		private static WebRequest GetPostFormRequest(string url) {
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.ContentType = ContentType.Form;
			request.Method = "POST";
			return request;
		}

		private static WebRequest GetPostJsonRequest(string url) {
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
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
	}
}
