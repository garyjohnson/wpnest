namespace WPNest.Services {

	public class Thermostat {

		public Thermostat(string id) {
			ID = id;
		}

		public string ID { get; private set; }
		public double TargetTemperature { get; set; }
		public double CurrentTemperature { get; set; }
		public bool IsHeating { get; set; }
		public bool IsCooling { get; set; }
		public FanMode FanMode { get; set; }
		public bool IsLeafOn { get; set; }
		public HvacMode HvacMode { get; set; }
	}
}
