using System;

namespace WPNest.Services {

	public class LoginResult : WebServiceResult {

		public LoginResult() {
		}

		public LoginResult(Exception error) {
			Error = error;
		}

		public string AccessToken { get; set; }
		public string UserId { get; set; }
		public string TransportUrl { get; set; }
		public DateTime AccessTokenExpirationDate { get; set; }
	}
}
