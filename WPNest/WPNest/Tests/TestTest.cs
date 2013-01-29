using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WPNest.Services;

namespace WPNest.Tests {

	[TestClass]
	public class TestTest : WorkItemTest {

		[TestMethod]
		[Asynchronous]
		public async void Test() {

			var t = new Mock<INestWebService>();
			t.DefaultValue = DefaultValue.Mock;
			t.Setup(w => w.GetStatusAsync()).Returns(new Task<GetStatusResult>(() => new GetStatusResult(new List<Structure>())));
//			T t = new T();

			GetStatusResult s = await t.Object.GetStatusAsync();

			Assert.IsNotNull(s);
			EnqueueTestComplete();
		}

		public class T : INestWebService {

			public Task<WebServiceResult> LoginAsync(string userName, string password) {
				return new Task<WebServiceResult>(() => new WebServiceResult());
			}

			public Task<GetStatusResult> GetStatusAsync() {
				return new Task<GetStatusResult>(() => new GetStatusResult(new List<Structure>()));
			}

			public Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature) {
				return new Task<WebServiceResult>(() => new WebServiceResult());
			}

			public Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat) {
				return new Task<GetThermostatStatusResult>(() => new GetThermostatStatusResult());
			}

			public Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode) {
				return new Task<WebServiceResult>(() => new WebServiceResult());
			}

			public Task<WebServiceResult> UpdateTransportUrlAsync() {
				return new Task<WebServiceResult>(() => new WebServiceResult());
			}
		}
	}
}
