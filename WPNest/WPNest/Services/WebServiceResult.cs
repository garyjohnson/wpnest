using System;

namespace WPNest.Services {

	public class WebServiceResult {

		public WebServiceResult() {
		}

		public WebServiceResult(Exception exception) {
			Exception = exception;
		}

		public WebServiceResult(WebServiceError error, Exception exception) {
			Error = error;
			Exception = exception;
		}

		public Exception Exception { get; protected set; }
		public WebServiceError Error { get; protected set; }
	}

}
