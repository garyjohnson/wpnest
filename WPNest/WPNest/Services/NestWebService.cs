using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpGIS;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		public async Task<WebServiceResult> LoginAsync(string userName, string password) {
			WebRequest request = GetPostFormRequest("https://home.nest.com/user/login");

			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);

			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				CacheSession(responseString);
				return new WebServiceResult();
			}
			catch (Exception exception) {
				return new WebServiceResult(exception);
			}
		}

		public async Task<GetStatusResult> GetStatusAsync() {
			var session = ServiceContainer.GetService<ISessionProvider>();
			if (session.IsSessionExpired)
				return new GetStatusResult(new SessionExpiredException());

			string url = string.Format("{0}/v2/mobile/user.{1}", session.TransportUrl, session.UserId);
			var request = GetGetRequest(url);
			SetAuthorizationHeaderOnRequest(request, session.AccessToken);

			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				return ParseGetStatusResult(responseString, session.UserId);
			}
			catch (Exception exception) {
				return new GetStatusResult(exception);
			}
		}

		public async Task<WebServiceResult> RaiseTemperatureAsync(Thermostat thermostat) {
			double desiredTemperature = thermostat.Temperature + 1.0d;
			return await ChangeTemperatureAsync(thermostat, desiredTemperature);
		}

		public async Task<WebServiceResult> LowerTemperatureAsync(Thermostat thermostat) {
			double desiredTemperature = thermostat.Temperature - 1.0d;
			return await ChangeTemperatureAsync(thermostat, desiredTemperature);
		}

		public async Task<GetTemperatureResult> GetTemperatureAsync(Thermostat thermostat) {
			var session = ServiceContainer.GetService<ISessionProvider>();
			if (session.IsSessionExpired)
				return new GetTemperatureResult(new SessionExpiredException());

			string url = string.Format("{0}/v2/subscribe", session.TransportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, session.AccessToken);
			SetNestHeadersOnRequest(request, session.UserId);

			string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\"}}]}}", thermostat.ID);
			await request.SetRequestStringAsync(requestString);

			try {
				WebResponse response = await request.GetResponseAsync();
				string strContent = await response.GetResponseStringAsync();
				return ParseGetTemperatureResult(strContent);
			}
			catch (Exception exception) {
				return new GetTemperatureResult(exception);
			}
		}

		private static GetTemperatureResult ParseGetTemperatureResult(string strContent) {
			var values = JObject.Parse(strContent);
			double temperatureCelcius = double.Parse(values["target_temperature"].Value<string>());
			double temperature = Math.Round(temperatureCelcius.CelciusToFahrenheit());
			return new GetTemperatureResult(temperature);
		}

		private async Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature) {
			var session = ServiceContainer.GetService<ISessionProvider>();
			if (session.IsSessionExpired)
				return new GetStatusResult(new SessionExpiredException());

			string url = string.Format(@"{0}/v2/put/shared.{1}", session.TransportUrl, thermostat.ID);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, session.AccessToken);
			SetNestHeadersOnRequest(request, session.UserId);

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
					double temperature = double.Parse(values["shared"][thermostat.ID]["target_temperature"].Value<string>());
					thermostat.Temperature = Math.Round(temperature.CelciusToFahrenheit());
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
			string accessToken = values["access_token"].Value<string>();
			DateTime accessTokenExpirationDate = values["expires_in"].Value<DateTime>();
			string userId = values["userid"].Value<string>();
			string transportUrl = values["urls"]["transport_url"].Value<string>();

			var sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			sessionProvider.SetSession(transportUrl, userId, accessToken, accessTokenExpirationDate);
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
