using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpGIS;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		public async Task<LoginResult> LoginAsync(string userName, string password) {
			WebRequest request = GetPostFormRequest("https://home.nest.com/user/login");

			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);

			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				return ParseLoginResult(responseString);
			}
			catch (Exception exception) {
				return new LoginResult(exception);
			}
		}

		public async Task<GetStatusResult> GetStatusAsync(string transportUrl, string accessToken, string userId) {
			string url = string.Format("{0}/v2/mobile/user.{1}", transportUrl, userId);
			var request = GetGetRequest(url);
			SetAuthorizationHeaderOnRequest(request, accessToken);

			try {
				WebResponse response = await request.GetResponseAsync();
				string responseString = await response.GetResponseStringAsync();
				return ParseGetStatusResult(responseString, userId);
			}
			catch (Exception exception) {
				return new GetStatusResult(exception);
			}
		}

		public async Task<WebServiceResult> RaiseTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			double desiredTemperature = thermostat.Temperature + 1.0d;
			return await ChangeTemperatureAsync(transportUrl, accessToken, userId, thermostat, desiredTemperature);
		}

		public async Task<WebServiceResult> LowerTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			double desiredTemperature = thermostat.Temperature - 1.0d;
			return await ChangeTemperatureAsync(transportUrl, accessToken, userId, thermostat, desiredTemperature);
		}

		public async Task<GetTemperatureResult> GetTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			string url = string.Format("{0}/v2/subscribe", transportUrl);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, accessToken);
			SetNestHeadersOnRequest(request, userId);

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

		private async Task<WebServiceResult> ChangeTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat, double desiredTemperature) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", transportUrl, thermostat.ID);
			WebRequest request = GetPostJsonRequest(url);
			SetAuthorizationHeaderOnRequest(request, accessToken);
			SetNestHeadersOnRequest(request, userId);

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

		private LoginResult ParseLoginResult(string responseString) {
			var values = JObject.Parse(responseString);
			return new LoginResult {
				AccessToken = values["access_token"].Value<string>(),
				AccessTokenExpirationDate = values["expires_in"].Value<DateTime>(),
				UserId = values["userid"].Value<string>(),
				Email = values["email"].Value<string>(),
				TransportUrl = values["urls"]["transport_url"].Value<string>()
			};
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
