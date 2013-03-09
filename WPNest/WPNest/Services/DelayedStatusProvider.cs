using System;
using System.Threading;
using System.Windows;

namespace WPNest.Services {

	internal class DelayedStatusProvider : IStatusProvider {

		private readonly Timer _displayCachedStatusTimer;
		private GetStatusResult _cachedStatusResult;

		public DelayedStatusProvider() {
			_displayCachedStatusTimer = new Timer(OnDisplayCachedStatusTick);
			StartDisplayCachedStatusTimer();
		}

		public void CacheStatus(GetStatusResult status) {
			_cachedStatusResult = status;
		}

		public void Reset() {
			_cachedStatusResult = null;
		}

		public event EventHandler<StatusEventArgs> StatusUpdated;

		private void OnDisplayCachedStatusTick(object state) {
			try {
				Deployment.Current.Dispatcher.InvokeAsync(() => {
					GetStatusResult cachedStatus = _cachedStatusResult;
					if (cachedStatus != null) {

						if (StatusUpdated != null)
							StatusUpdated(this, new StatusEventArgs(cachedStatus));

						_cachedStatusResult = null;
					}
				});
			}
			catch (NullReferenceException) {}
		}

		private void StartDisplayCachedStatusTimer() {
			_displayCachedStatusTimer.Change(1000, 1000);
		}
	}
}
