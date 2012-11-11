using System;

namespace WPNest.Login {

	public class GetTemperatureResult {

		public GetTemperatureResult(double temperature) {
			Temperature = temperature;
		}

		public GetTemperatureResult(Exception error) {
			Error = error;
		}

		public Exception Error { get; private set; }
		public double Temperature { get; set; }
	}
}
