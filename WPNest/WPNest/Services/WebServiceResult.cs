using System;

namespace WPNest.Services {

	public class WebServiceResult {

		public WebServiceResult() {
		}

		public WebServiceResult(Exception error) {
			Error = error;
		}

		public Exception Error { get; protected set; }
	}

}
