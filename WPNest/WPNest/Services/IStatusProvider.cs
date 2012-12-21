using System;

namespace WPNest.Services {

	public interface IStatusProvider {
		void CacheThermostatStatus(GetThermostatStatusResult thermostatStatus);
		void Reset();
		event EventHandler<ThermostatStatusEventArgs> ThermostatStatusUpdated;
	}
}
