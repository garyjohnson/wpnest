using System;

namespace WPNest.Services {

	public interface IAnalyticsService {
		void StartSession();
		void EndSession();
		void LogError(Exception exception);
		void LogEvent(string eventName, params object[] parameters);
	}
}
