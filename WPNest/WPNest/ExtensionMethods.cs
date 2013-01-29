using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPNest {

	public static class ExtensionMethods {

		public static void SetRequestStringAndThenExecute(this WebRequest request, string requestString, object userState, Action<WebRequest, object> onCompleted) {
			var state = new Dictionary<string, object> {
				            {"request", request},
				            {"userState", userState},
				            {"onCompleted", onCompleted},
							{"requestString", requestString}};

			request.BeginGetRequestStream(ContinueSetRequestString, state);
		}

		private static void ContinueSetRequestString(IAsyncResult result) {
			var state = (Dictionary<string, object>)result.AsyncState;
			var request = (WebRequest)state["request"];
			var userState = state["userState"];
			var onCompleted = (Action<WebRequest, object>)state["onCompleted"];
			var requestString = (string)state["requestString"];

			using (Stream stream = request.EndGetRequestStream(result)) {
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestString);
				stream.Write(encodedRequestString, 0, encodedRequestString.Length);
			}

			onCompleted(request, userState);
		}

		public static async Task SetRequestStringAsync(this WebRequest request, string requestString) {
			using (Stream stream = await request.GetRequestStreamAsync()) {
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestString);
				await stream.WriteAsync(encodedRequestString, 0, encodedRequestString.Length);
			}
		}

		public static async Task<string> GetResponseStringAsync(this WebRequest request) {
			WebResponse response = await request.GetResponseAsync();
			Stream responseStream = response.GetResponseStream();
			using (var streamReader = new StreamReader(responseStream)) {
				return streamReader.ReadToEnd();
			}
		}

		public static string GetResponseString(this WebResponse response) {
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = sr.ReadToEnd();
			}

			return strContent;
		}

		public static async Task<string> GetResponseStringAsync(this WebResponse response) {
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
//				var builder = new StringBuilder();
//				var buffer = new char[128];
//				while(!sr.EndOfStream) {
//					sr.Read(buffer, 0, 128);
////					await sr.ReadAsync(buffer, 0, 128);
//					builder.Append(buffer);
//					buffer = new char[128];
//				}
					
				strContent = await sr.ReadToEndAsync();
//				strContent = builder.ToString();
			}

			return strContent;
		}

		public static string UrlEncode(this string value) {
			return HttpUtility.UrlEncode(value);
		}

		public static double CelciusToFahrenheit(this double celcius) {
			return (celcius * 1.8d) + 32.0d;
		}

		public static double FahrenheitToCelcius(this double fahrenheit) {
			return (fahrenheit - 32.0d) / 1.8d;
		}
	}
}
