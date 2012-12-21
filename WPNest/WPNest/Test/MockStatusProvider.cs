using System;
using WPNest.Services;

namespace WPNest.Test {
	public class MockStatusProvider : IStatusProvider {

		public void CacheThermostatStatus(GetThermostatStatusResult thermostatStatus) {
		}

		public void Reset() {
		}

		public void FireThermostatStatusUpdated(GetThermostatStatusResult result) {
			if(ThermostatStatusUpdated != null) ThermostatStatusUpdated(this, new ThermostatStatusEventArgs(result));
		}

		public event EventHandler<ThermostatStatusEventArgs> ThermostatStatusUpdated;
	}
}
