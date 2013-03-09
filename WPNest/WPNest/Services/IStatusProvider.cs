using System;

namespace WPNest.Services {

	public interface IStatusProvider {
		void CacheStatus(GetStatusResult status);
		void Reset();
		event EventHandler<StatusEventArgs> StatusUpdated;
	}
}
