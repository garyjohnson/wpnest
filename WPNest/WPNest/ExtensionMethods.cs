using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPNest {

	public static class ExtensionMethods {

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

		public static async Task<string> GetResponseStringAsync(this WebResponse response) {
			Stream responseStream = response.GetResponseStream();
			string strContent = "";
			using (var sr = new StreamReader(responseStream)) {
				strContent = await sr.ReadToEndAsync();
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
