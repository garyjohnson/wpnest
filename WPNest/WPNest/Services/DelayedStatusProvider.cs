using System;
using System.Threading;
using System.Windows;

namespace WPNest.Services {

	internal class DelayedStatusProvider : IStatusProvider {

		private readonly Timer _displayCachedStatusTimer;
		private GetThermostatStatusResult _cachedThermostatStatus;

		public DelayedStatusProvider() {
			_displayCachedStatusTimer = new Timer(OnDisplayCachedStatusTick);
			StartDisplayCachedStatusTimer();
		}

		public void CacheThermostatStatus(GetThermostatStatusResult thermostatStatus) {
			_cachedThermostatStatus = thermostatStatus;
		}

		public void Reset() {
			_cachedThermostatStatus = null;
		}

		public event EventHandler<ThermostatStatusEventArgs> ThermostatStatusUpdated;

		private void OnDisplayCachedStatusTick(object state) {
			try {
				Deployment.Current.Dispatcher.InvokeAsync(() => {
					GetThermostatStatusResult cachedStatus = _cachedThermostatStatus;
					if (cachedStatus != null) {

						if (ThermostatStatusUpdated != null)
							ThermostatStatusUpdated(this, new ThermostatStatusEventArgs(cachedStatus));

						_cachedThermostatStatus = null;
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
