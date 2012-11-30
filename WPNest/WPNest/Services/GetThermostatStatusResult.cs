﻿using System;

namespace WPNest.Services {

	public class GetThermostatStatusResult : WebServiceResult {

		public GetThermostatStatusResult(double targetTemperature, double currentTemperature, bool isHeating, bool isCooling) {
			TargetTemperature = targetTemperature;
			CurrentTemperature = currentTemperature;
			IsHeating = isHeating;
			IsCooling = isCooling;
		}

		public GetThermostatStatusResult(Exception error) {
			Error = error;
		}

		public bool IsHeating { get; private set; }
		public bool IsCooling { get; private set; }
		public double TargetTemperature { get; private set; }
		public double CurrentTemperature { get; private set; }
	}
}