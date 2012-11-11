using System;
using System.Collections.Generic;

namespace WPNest.Services {

	public class SessionProvider : ISessionProvider {

		private const string SettingTransportUrl = "TransportUrl";
		private const string SettingUserId = "UserId";
		private const string SettingAccessToken = "AccessToken";
		private const string SettingAccessTokenExpirationDate = "AccessTokenExpirationDate";

		private DateTime? _accessTokenExpirationDate;
		public string TransportUrl { get; private set; }
		public string AccessToken { get; private set; }
		public string UserId { get; private set; }

		public SessionProvider() {
			LoadSettings();
		}

		private void LoadSettings() {
			var settings = ServiceContainer.GetService<ISettingsProvider>();
			foreach (var setting in SettingsKeys) {
				if (!settings.ContainsKey(setting)) {
					ClearSession();
					return;
				}
			}

			TransportUrl = (string)settings[SettingTransportUrl];
			UserId = (string)settings[SettingUserId];
			AccessToken = (string)settings[SettingAccessToken];
			_accessTokenExpirationDate = (DateTime?)settings[SettingAccessTokenExpirationDate];
		}

		public bool IsSessionExpired {
			get {
				ClearTokenIfPastExpirationDate();
				return AccessToken == null;
			}
		}

		private void ClearTokenIfPastExpirationDate() {
			if (_accessTokenExpirationDate.HasValue && DateTime.Now > _accessTokenExpirationDate.Value)
				ClearSession();
		}

		public void SetSession(string transportUrl, string userId, string accessToken, DateTime accessTokenExpirationDate) {
			var settings = ServiceContainer.GetService<ISettingsProvider>();
			settings[SettingTransportUrl] = transportUrl;
			settings[SettingUserId] = userId;
			settings[SettingAccessToken] = accessToken;
			settings[SettingAccessTokenExpirationDate] = accessTokenExpirationDate;
			settings.Save();

			LoadSettings();
		}

		public void ClearSession() {
			TransportUrl = null;
			AccessToken = null;
			UserId = null;
			_accessTokenExpirationDate = null;

			var settings = ServiceContainer.GetService<ISettingsProvider>();
			foreach (var setting in SettingsKeys)
				settings.Remove(setting);

			settings.Save();
		}

		private IEnumerable<string> SettingsKeys {
			get {
				yield return SettingAccessToken;
				yield return SettingAccessTokenExpirationDate;
				yield return SettingTransportUrl;
				yield return SettingUserId;
			}
		}
	}
}
