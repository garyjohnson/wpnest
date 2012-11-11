using System;

namespace WPNest.Services {

	public class GetTemperatureResult : WebServiceResult {

		public GetTemperatureResult(double temperature) {
			Temperature = temperature;
		}

		public GetTemperatureResult(Exception error) {
			Error = error;
		}

		public double Temperature { get; private set; }
	}
}
