using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SharpGIS;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		private const string ContentTypeForm = @"application/x-www-form-urlencoded; charset=utf-8";
		private const string ContentTypeJson = @"application/json";

		private static readonly Uri LoginUri = new Uri("https://home.nest.com/user/login");

		public async Task<LoginResult> LoginAsync(string userName, string password) {
			WebRequest request = GetPostFormRequest();
			string requestString = string.Format("username={0}&password={1}", UrlEncode(userName), UrlEncode(password));
			await request.SetRequestStringAsync(requestString);

			WebResponse response;
			try {
				response = await request.GetResponseAsync();
			}
			catch (WebException webException) {
				return new LoginResult(webException);
			}

			string responseString = await response.GetResponseStringAsync();
			return ParseLoginResult(responseString);
		}

		public async Task<GetStatusResult> GetStatusAsync(string transportUrl, string accessToken, string user) {
			string url = string.Format("{0}/v2/mobile/{1}", transportUrl, user);
			var request = GetGetRequest(url);
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			WebResponse response;
			try {
				response = await request.GetResponseAsync();
			}
			catch (Exception webException) {
				return new GetStatusResult(webException);
			}
			string responseString = await response.GetResponseStringAsync();
			return ParseGetStatusResult(responseString, user);
		}

		public async Task<WebServiceResult> RaiseTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			double desiredTempFahrenheit = thermostat.Temperature + 1.0d;
			return await ChangeTemperatureAsync(transportUrl, accessToken, userId, thermostat, desiredTempFahrenheit);
		}

		public async Task<WebServiceResult> LowerTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			double desiredTempFahrenheit = thermostat.Temperature - 1.0d;
			return await ChangeTemperatureAsync(transportUrl, accessToken, userId, thermostat, desiredTempFahrenheit);
		}

		private async Task<WebServiceResult> ChangeTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat, double desiredTemperature) {
			string url = string.Format(@"{0}/v2/put/shared.{1}", transportUrl, thermostat.ID);
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.ContentType = ContentTypeJson;
			request.Method = "POST";
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			request.Headers["X-nl-base-version"] = "1190775996";
			request.Headers["X-nl-protocol-version"] = "1";
			request.Headers["X-nl-user-id"] = userId;
			request.Headers["X-nl-session-id"] = string.Format("ios-{0}-373941569.382847", userId);
			request.Headers["X-nl-merge-payload"] = "true";

			double desiredTempCelcius = desiredTemperature.FahrenheitToCelcius();
			string requestString = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTempCelcius.ToString());
			await request.SetRequestStringAsync(requestString);

			try {
				await request.GetResponseAsync();
			}
			catch (WebException webException) {
				return new WebServiceResult(webException);
			}

			return new WebServiceResult();
		}

		private static GetStatusResult ParseGetStatusResult(string responseString, string user) {
			var structureResults = new List<Structure>();

			var values = JObject.Parse(responseString);
			var structures = values["user"][user]["structures"];
			foreach (var structure in structures) {
				structureResults.Add(new Structure(structure.Value<string>()));
			}

			foreach (var structureResult in structureResults) {
				var devices = values["structure"][structureResult.ID]["devices"];
				foreach (var device in devices) {
					string id = device.Value<string>().Replace("device.", "");
					var thermostat = new Thermostat(id);
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

		private static WebRequest GetPostFormRequest() {
			WebRequest request = WebRequestCreator.GZip.Create(LoginUri);
			request.ContentType = ContentTypeForm;
			request.Method = "POST";
			return request;
		}

		private LoginResult ParseLoginResult(string responseString) {
			var values = JObject.Parse(responseString);
			return new LoginResult {
				AccessToken = values["access_token"].Value<string>(),
				AccessTokenExpirationDate = values["expires_in"].Value<DateTime>(),
				User = values["user"].Value<string>(),
				UserId = values["userid"].Value<string>(),
				Email = values["email"].Value<string>(),
				TransportUrl = values["urls"]["transport_url"].Value<string>()
			};
		}

		private string UrlEncode(string value) {
			return HttpUtility.UrlEncode(value);
		}
	}
}
