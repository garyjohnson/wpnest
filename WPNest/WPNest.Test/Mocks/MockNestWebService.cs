using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPNest.Services;

namespace WPNest.Test.Mocks {

	public class MockNestWebService : INestWebService {

		public Task<WebServiceResult> LoginAsync(string userName, string password) {
			return Task.FromResult(new WebServiceResult());
		}

		public Task<GetStatusResult> GetStatusAsync() {
			return Task.FromResult(new GetStatusResult(WebServiceError.None, null));
		}

		public Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature) {
			return Task.FromResult(new WebServiceResult());
		}

		public Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat) {
			return Task.FromResult(new GetThermostatStatusResult());
		}

		public Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode) {
			return Task.FromResult(new WebServiceResult());
		}

		public Task<WebServiceResult> UpdateTransportUrlAsync() {
			return Task.FromResult(new WebServiceResult());
		}
	}
}
