using System;
using System.Net;
using WPNest.Web;

namespace WPNest.Services {

	internal class WebRequestProvider : IWebRequestProvider {

		public IWebRequest CreateRequest(Uri uri) {
			WebRequest request = WebRequest.Create(uri);
			return new WebRequestWrapper(request);
		}
	}
}
