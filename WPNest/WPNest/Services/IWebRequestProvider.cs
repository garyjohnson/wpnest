using System;
using WPNest.Web;

namespace WPNest.Services {

	public interface IWebRequestProvider {
		IWebRequest CreateRequest(Uri uri);
	}
}
