using System.Collections.Generic;

namespace WPNest.Services {

	public class Structure {

		public Structure(string id) {
			ID = id;
		}

		public string ID { get; private set; }

		private readonly List<Thermostat> thermostats = new List<Thermostat>();
		public List<Thermostat> Thermostats {
			get { return thermostats; }
		}
	}
}
