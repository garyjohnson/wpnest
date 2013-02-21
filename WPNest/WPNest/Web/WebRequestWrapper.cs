using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WPNest.Web {

	internal class WebRequestWrapper : IWebRequest {

		private readonly WebRequest _request;

		public WebRequestWrapper(WebRequest request) {
			if(request == null) throw new ArgumentNullException("request");
			_request = request;
		}

		public string Method {
			get { return _request.Method; }
			set { _request.Method = value; }
		}

		public string ContentType {
			get { return _request.ContentType; }
			set { _request.ContentType = value; }
		}

		public WebHeaderCollection Headers {
			get { return _request.Headers; }
			set { _request.Headers = value; }
		}

		public async Task<Stream> GetRequestStreamAsync() {
			return await _request.GetRequestStreamAsync();
		}

		public async Task<IWebResponse> GetResponseAsync() {
			WebResponse response = await _request.GetResponseAsync();
			return new WebResponseWrapper(response);
		}

		public async Task SetRequestStringAsync(string requestString) {
			await _request.SetRequestStringAsync(requestString);
		}
	}
}
