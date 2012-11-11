using System;

namespace WPNest.Services {

	public interface ISessionProvider {

		bool IsSessionExpired { get; }
		void SetSession(string transportUrl, string userId, string accessToken, DateTime accessTokenExpirationDate);
		void ClearSession();

		string TransportUrl { get; }
		string AccessToken { get; }
		string UserId { get; }
	}
}
