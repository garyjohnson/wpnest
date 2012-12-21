using System;
using WPNest.Services;

namespace WPNest.Test {

	public class MockSessionProvider : ISessionProvider {

		public void SetSession(string transportUrl, string userId, string accessToken, DateTime accessTokenExpirationDate) {
		}

		public void ClearSession() {
		}

		public bool IsSessionExpired { get; set; }
		public string TransportUrl { get; set; }
		public string AccessToken { get; set; }
		public string UserId { get; set; }
	}
}
