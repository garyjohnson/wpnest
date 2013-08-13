using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WPNest.Web;

namespace WPNest {

	internal static class ExtensionMethods {

		public static async Task SetRequestStringAsync(this WebRequest request, string requestString) {
			using (Stream stream = await request.GetRequestStreamAsync()) {
				byte[] encodedRequestString = Encoding.UTF8.GetBytes(requestString);
				await stream.WriteAsync(encodedRequestString, 0, encodedRequestString.Length);
			}
		}

		public static async Task<string> GetResponseStringAsync(this WebRequest request) {
			WebResponse response = await request.GetResponseAsync();
			return await response.GetResponseStringAsync();
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

		public static double CelsiusToFahrenheit(this double celsius) {
			return (celsius * 1.8d) + 32.0d;
		}

		public static double FahrenheitToCelsius(this double fahrenheit) {
			return (fahrenheit - 32.0d) / 1.8d;
		}
	}
}
