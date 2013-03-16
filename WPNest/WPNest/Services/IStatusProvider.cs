using System;

namespace WPNest.Services {

	public interface IStatusProvider {
		void CacheStatus(GetStatusResult status);
		void Start();
		void Stop();
		event EventHandler<StatusEventArgs> StatusUpdated;
	}
}
