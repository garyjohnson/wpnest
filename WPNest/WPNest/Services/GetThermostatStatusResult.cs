using System;

namespace WPNest.Services {

	public class GetThermostatStatusResult : WebServiceResult {

		public GetThermostatStatusResult(Thermostat thermostat) {
			Thermostat = thermostat;
		}

		public GetThermostatStatusResult(WebServiceError error, Exception exception) {
			Error = error;
			Exception = exception;
		}

		public Thermostat Thermostat { get; private set; }
	}
}
