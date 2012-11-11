using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using SharpGIS;
using WPNest.Login;

namespace WPNest.Services {

	public class NestWebService : INestWebService {

		private const string ContentTypeForm = @"application/x-www-form-urlencoded; charset=utf-8";

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

		public async Task<GetTemperatureResult> GetTemperatureAsync(string transportUrl, string accessToken, string user) {
			string url = string.Format("{0}/v2/mobile/{1}", transportUrl, user);
			var request = GetGetRequest(url);
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			WebResponse response;
			try {
				response = await request.GetResponseAsync();
			}
			catch (Exception webException) {
				return new GetTemperatureResult(webException);
			}
			string responseString = await response.GetResponseStringAsync();
			return ParseGetTemperatureResult(responseString);
		}

//		public void Up() {
//			string url = string.Format(@"{0}/v2/put/shared.{1}", transportUrl, firstDeviceId);
//			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
//			request.ContentType = @"application/json";
//			request.Method = "POST";
//			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);
//
//			request.Headers["X-nl-base-version"] = "1190775996";
//			request.Headers["X-nl-protocol-version"] = "1";
//			request.Headers["X-nl-user-id"] = userId;
//			request.Headers["X-nl-session-id"] = string.Format("ios-{0}-373941569.382847", userId);
//			request.Headers["X-nl-merge-payload"] = "true";
//
//			request.BeginGetRequestStream(UpGetRequestStreamCallback, request);
//		}
//
//		private void UpGetRequestStreamCallback(IAsyncResult result) {
//			var request = (WebRequest)result.AsyncState;
//			using (Stream requestStream = request.EndGetRequestStream(result)) {
//
//				_temperature += 1.0d;
//				double desiredTemp = FahrenheitToCelcius(_temperature);
//				string t = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTemp.ToString());
//				byte[] encodedRequestString = Encoding.UTF8.GetBytes(t);
//				requestStream.Write(encodedRequestString, 0, encodedRequestString.Length);
//
//			}
//			request.BeginGetResponse(UpGetResponseCallback, request);
//		}
//
//		private void UpGetResponseCallback(IAsyncResult result) {
//			var request = (WebRequest)result.AsyncState;
//			WebResponse response = request.EndGetResponse(result);
//			Stream responseStream = response.GetResponseStream();
//			string strContent = "";
//			using (var sr = new StreamReader(responseStream)) {
//				strContent = sr.ReadToEnd();
//			}
//
////			Refresh();
//		}

		private static GetTemperatureResult ParseGetTemperatureResult(string responseString) {
			var values = JObject.Parse(responseString);
			var shared = values["shared"];
			var firstDevice = (JProperty) shared.First;
			string firstDeviceId = firstDevice.Name;
			double temperature = double.Parse(shared[firstDeviceId]["target_temperature"].Value<string>());
			return new GetTemperatureResult(Math.Round(temperature.CelciusToFahrenheit()));
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
				User = values["user"].Value<string>(),
				UserId = values["userid"].Value<string>(),
				TransportUrl = values["urls"]["transport_url"].Value<string>()
			};
		}

		private string UrlEncode(string value) {
			return HttpUtility.UrlEncode(value);
		}
	}
}
