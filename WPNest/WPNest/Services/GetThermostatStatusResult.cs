using System;

namespace WPNest.Services {

	public class GetThermostatStatusResult : WebServiceResult {

		public GetThermostatStatusResult() {
		}

		public GetThermostatStatusResult(WebServiceError error, Exception exception) {
			Error = error;
			Exception = exception;
		}

		public bool IsHeating { get; set; }
		public bool IsCooling { get; set; }
		public double TargetTemperature { get; set; }
		public double CurrentTemperature { get; set; }

		public FanMode FanMode { get; set; }
		public HvacMode HvacMode { get; set; }
		public bool IsAway { get; set; }
	}
}
