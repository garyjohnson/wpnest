using System;
#if WP7
using FlurryWP7SDK;
#elif WP8
using FlurryWP8SDK;
#endif

namespace WPNest.Services {

	internal class AnalyticsService : IAnalyticsService {

		public void StartSession() {
			Api.StartSession("ZY7JNH8M6C4PKKMYDXJH");
		}

		public void EndSession() {
			Api.EndSession();
		}

		public void LogError(Exception exception) {
			Api.LogError(exception.Message, exception);
		}

		public void LogEvent(string eventName, params object[] parameters) {
			Api.LogEvent(string.Format(eventName, parameters));
		}
	}
}
