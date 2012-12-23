using System;

namespace WPNest.Services {

	public class AnalyticsService : IAnalyticsService {

		public void StartSession() {
			FlurryWP7SDK.Api.StartSession("ZY7JNH8M6C4PKKMYDXJH");
		}

		public void EndSession() {
			FlurryWP7SDK.Api.EndSession();
		}

		public void LogError(Exception exception) {
			FlurryWP7SDK.Api.LogError(exception.Message, exception);
		}

		public void LogEvent(string eventName, params object[] parameters) {
			FlurryWP7SDK.Api.LogEvent(string.Format(eventName, parameters));
		}
	}
}
