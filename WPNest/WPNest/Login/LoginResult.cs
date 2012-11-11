using System;

namespace WPNest.Login {

	public class LoginResult {

		public LoginResult() {
		}

		public LoginResult(Exception error) {
			Error = error;
		}

		public Exception Error { get; private set; }
		public string AccessToken { get; set; }
		public string User { get; set; }
		public string UserId { get; set; }
		public string TransportUrl { get; set; }
	}
}
