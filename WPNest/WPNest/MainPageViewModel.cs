using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using SharpGIS;

namespace WPNest {

	public class MainPageViewModel : INotifyPropertyChanged {

		private string accessToken = "";
		private string transportUrl = "";
		private string userId = "";
		private string user = "";
		private string firstDeviceId = "";

		private double _temperature;

		private string _currentTemperature = "0";
		public string CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged("CurrentTemperature");
			}
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn {
			get { return _isLoggedIn; }
			set {
				_isLoggedIn = value;
				OnPropertyChanged("IsLoggedIn");
			}
		}

		private string _userName = "";
		public string UserName {
			get { return _userName; }
			set {
				_userName = value;
				OnPropertyChanged("UserName");
			}
		}

		private string _password = "";
		public string Password {
			get { return _password; }
			set {
				_password = value;
				OnPropertyChanged("Password");
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private double CelciusToFahrenheit(double celcius) {
			return (celcius * 1.8d) + 32.0d;
		}

		private double FahrenheitToCelcius(double fahrenheit) {
			return (fahrenheit - 32.0d) / 1.8d;
		}

		public void Login() {
			WebRequest request = WebRequestCreator.GZip.Create(new Uri("https://home.nest.com/user/login"));
			request.ContentType = @"application/x-www-form-urlencoded; charset=utf-8";
			request.Method = "POST";

			request.BeginGetRequestStream(LoginGetRequestStreamCallback, request);
		}

		private void LoginGetRequestStreamCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			using (Stream requestStream = request.EndGetRequestStream(result)) {
				string usernameEncoded = HttpUtility.UrlEncode(UserName);
				string passwordEncoded = HttpUtility.UrlEncode(Password);
				string requestString = string.Format("username={0}&password={1}", usernameEncoded, passwordEncoded);

				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestString);
				requestStream.Write(encodedRequestString, 0, encodedRequestString.Length);

			}
			request.BeginGetResponse(LoginGetResponseCallback, request);
		}

		private void LoginGetResponseCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			WebResponse response = request.EndGetResponse(result);
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = sr.ReadToEnd();
			}

			var values = JObject.Parse(strContent);
			accessToken = values["access_token"].Value<string>();
			user = values["user"].Value<string>();
			userId = values["userid"].Value<string>();
			var urls = values["urls"];
			transportUrl = urls["transport_url"].Value<string>();

			GetInfo();
		}

		public void GetInfo() {
			string url = string.Format("{0}/v2/mobile/{1}", transportUrl, user);
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.Method = "GET";
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			request.BeginGetResponse(GetInfoGetResponseCallback, request);
		}

		private void GetInfoGetResponseCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			WebResponse response = request.EndGetResponse(result);
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = sr.ReadToEnd();
			}

			var values = JObject.Parse(strContent);
			var shared = values["shared"];
			var first = (JProperty)shared.First;
			firstDeviceId = first.Name;

			double temp = double.Parse(shared[firstDeviceId]["target_temperature"].Value<string>());
			_temperature = Math.Round(CelciusToFahrenheit(temp));

			Deployment.Current.Dispatcher.BeginInvoke(() => {
				CurrentTemperature = _temperature.ToString();
				IsLoggedIn = true;
			});

		}

		public void Up() {
			string url = string.Format(@"{0}/v2/put/shared.{1}", transportUrl, firstDeviceId);
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.ContentType = @"application/json";
			request.Method = "POST";
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			request.Headers["X-nl-base-version"] = "1190775996";
			request.Headers["X-nl-protocol-version"] = "1";
			request.Headers["X-nl-user-id"] = userId;
			request.Headers["X-nl-session-id"] = string.Format("ios-{0}-373941569.382847", userId);
			request.Headers["X-nl-merge-payload"] = "true";

			request.BeginGetRequestStream(UpGetRequestStreamCallback, request);
		}

		private void UpGetRequestStreamCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			using (Stream requestStream = request.EndGetRequestStream(result)) {

				_temperature += 1.0d;
				double desiredTemp = FahrenheitToCelcius(_temperature);
				string t = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTemp.ToString());
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(t);
				requestStream.Write(encodedRequestString, 0, encodedRequestString.Length);

			}
			request.BeginGetResponse(UpGetResponseCallback, request);
		}

		private void UpGetResponseCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			WebResponse response = request.EndGetResponse(result);
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = sr.ReadToEnd();
			}

			Refresh();
		}

		public void Down() {
			string url = string.Format(@"{0}/v2/put/shared.{1}", transportUrl, firstDeviceId);
			WebRequest request = WebRequestCreator.GZip.Create(new Uri(url));
			request.ContentType = @"application/json";
			request.Method = "POST";
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);

			request.Headers["X-nl-base-version"] = "1190775996";
			request.Headers["X-nl-protocol-version"] = "1";
			request.Headers["X-nl-user-id"] = userId;
			request.Headers["X-nl-session-id"] = string.Format("ios-{0}-373941569.382847", userId);
			request.Headers["X-nl-merge-payload"] = "true";

			request.BeginGetRequestStream(DownGetRequestStreamCallback, request);
		}

		private void DownGetRequestStreamCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			using (Stream requestStream = request.EndGetRequestStream(result)) {

				_temperature -= 1.0d;
				double desiredTemp = FahrenheitToCelcius(_temperature);
				string t = string.Format("{{\"target_change_pending\":true,\"target_temperature\":{0}}}", desiredTemp.ToString());
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(t);
				requestStream.Write(encodedRequestString, 0, encodedRequestString.Length);

			}
			request.BeginGetResponse(DownGetResponseCallback, request);
		}

		private void DownGetResponseCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			WebResponse response = request.EndGetResponse(result);
			Stream responseStream = response.GetResponseStream();

			Refresh();
		}

		public void Refresh() {
			string url = string.Format("{0}/v2/subscribe", transportUrl);
			HttpWebRequest request = HttpWebRequest.CreateHttp(url);
			request.Method = "POST";
			request.Headers["Authorization"] = string.Format("Basic {0}", accessToken);
			request.Headers["X-nl-base-version"] = "1190775996";
			request.Headers["X-nl-protocol-version"] = "1";
			request.Headers["X-nl-user-id"] = userId;
			request.Headers["X-nl-session-id"] = string.Format("ios-{0}-373941569.382847", userId);
			request.Headers["X-nl-merge-payload"] = "true";

			request.BeginGetRequestStream(RefreshGetRequestStreamCallback, request);
		}

		private void RefreshGetRequestStreamCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			using (Stream requestStream = request.EndGetRequestStream(result)) {
				string requestString = string.Format("{{\"keys\":[{{\"key\":\"shared.{0}\",\"version\":-2029154136,\"timestamp\":1352247117000}}]}}", firstDeviceId);

				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestString);
				requestStream.Write(encodedRequestString, 0, encodedRequestString.Length);

			}
			request.BeginGetResponse(RefreshGetResponseCallback, request);
		}

		private void RefreshGetResponseCallback(IAsyncResult result) {
			var request = (WebRequest)result.AsyncState;
			WebResponse response = request.EndGetResponse(result);
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = sr.ReadToEnd();
			}

			var values = JObject.Parse(strContent);
			double temp = double.Parse(values["target_temperature"].Value<string>());
			_temperature = CelciusToFahrenheit(temp);

			Deployment.Current.Dispatcher.BeginInvoke(() => {
				CurrentTemperature = Math.Round(_temperature).ToString();
				IsLoggedIn = true;
			});

		}
	}
}
