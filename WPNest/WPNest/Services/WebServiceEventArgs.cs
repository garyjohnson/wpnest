using System;

namespace WPNest.Services {

	public class WebServiceEventArgs : EventArgs {

		public WebServiceEventArgs(WebServiceResult result) {
			Result = result;
		}

		public WebServiceResult Result { get; private set; }
	}
}
