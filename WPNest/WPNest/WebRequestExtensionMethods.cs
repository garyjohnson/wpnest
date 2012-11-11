using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WPNest {

	public static class WebRequestExtensionMethods {

		public static async Task<Stream> GetRequestStreamAsync(this WebRequest request) {
			return await new Task<Stream>(() => {
				IAsyncResult result = request.BeginGetRequestStream(ar => { }, null);
				return request.EndGetRequestStream(result);
			});
		}

		public static async Task<WebResponse> GetResponseAsync(this WebRequest request) {
			return await new Task<WebResponse>(() => {
				IAsyncResult result = request.BeginGetResponse(ar => { }, null);
				return request.EndGetResponse(result);
			});
		}
	}
}
