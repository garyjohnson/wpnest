using System;

namespace WPNest.Services {
	public class ThermostatStatusEventArgs : EventArgs {

		public GetThermostatStatusResult ThermostatStatus { get; private set; }
		public ThermostatStatusEventArgs(GetThermostatStatusResult thermostatStatus) {
			ThermostatStatus = thermostatStatus;
		}

	}
}
