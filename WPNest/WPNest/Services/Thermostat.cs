namespace WPNest.Services {

	public class Thermostat {

		public Thermostat(string id) {
			ID = id;
		}

		public string ID { get; private set; }
		public double Temperature { get; set; }
	}
}
