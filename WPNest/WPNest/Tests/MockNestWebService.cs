using System;
using System.Threading.Tasks;
using WPNest.Services;

namespace WPNest.Tests {

	public class MockNestWebService : INestWebService {

		public bool WasLoginCalled { get; private set; }

		public Task<GetStatusResult> GetStatusAsync(string transportUrl, string accessToken, string userId) {
			throw new NotImplementedException();
		}

		public Task<WebServiceResult> RaiseTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			throw new NotImplementedException();
		}

		public Task<WebServiceResult> LowerTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			throw new NotImplementedException();
		}

		public Task<GetTemperatureResult> GetTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat) {
			throw new NotImplementedException();
		}

		public Task<LoginResult> LoginAsync(string userName, string password) {
			throw new NotImplementedException();
		}
	}
}
