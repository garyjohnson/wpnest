using System;
using System.IO;
using System.Net;

namespace WPNest.Web {
	internal class WebResponseWrapper : IWebResponse {

		private WebResponse _response;

		public WebResponseWrapper(WebResponse response) {
			if (response == null)
				throw new ArgumentNullException("response");
			_response = response;
		}

		public HttpStatusCode StatusCode {
			get { return ((HttpWebResponse)_response).StatusCode; }
		}

		public Stream GetResponseStream() {
			return _response.GetResponseStream();
		}
	}
}
