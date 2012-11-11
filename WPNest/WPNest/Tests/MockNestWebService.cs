using System;
using System.Threading.Tasks;
using WPNest.Services;

namespace WPNest.Tests {

	public class MockNestWebService : INestWebService {

		public bool WasLoginCalled { get; private set; }

		public Task<WebServiceResult> LoginAsync(string userName, string password) {
			throw new NotImplementedException();
		}

		public Task<GetStatusResult> GetStatusAsync() {
			throw new NotImplementedException();
		}

		public Task<WebServiceResult> RaiseTemperatureAsync(Thermostat thermostat) {
			throw new NotImplementedException();
		}

		public Task<WebServiceResult> LowerTemperatureAsync(Thermostat thermostat) {
			throw new NotImplementedException();
		}

		public Task<GetTemperatureResult> GetTemperatureAsync(Thermostat thermostat) {
			throw new NotImplementedException();
		}
	}
}
