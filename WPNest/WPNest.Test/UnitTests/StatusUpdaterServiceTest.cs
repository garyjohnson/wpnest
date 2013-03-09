using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class StatusUpdaterServiceTest {

		private Mock<ITimer> _mockTimer;
		private Mock<IStatusProvider> _mockStatusProvider;
		private Mock<INestWebService> _mockWebService;
		private StatusUpdaterService _updaterService;
		private TimerCallback _timerCallback;
		private GetStatusResult _cachedStatusResult;

		[TestInitialize]
		public void SetUp() {
			_mockTimer = new Mock<ITimer>();
			_mockStatusProvider = new Mock<IStatusProvider>();
			_mockWebService = new Mock<INestWebService>();

			ServiceContainer.RegisterService<ITimer>(_mockTimer.Object);
			ServiceContainer.RegisterService<INestWebService>(_mockWebService.Object);
			ServiceContainer.RegisterService<IStatusProvider>(_mockStatusProvider.Object);

			_mockWebService.Setup(w => w.GetThermostatStatusAsync(It.IsAny<Thermostat>())).Returns(Task.FromResult(new GetThermostatStatusResult(new Thermostat(""))));
			_mockWebService.Setup(w => w.GetStructureAndDeviceStatusAsync(It.IsAny<Structure>())).Returns(Task.FromResult(new GetStatusResult(new[] { new Structure("") })));
			_mockTimer.Setup(t => t.SetCallback(It.IsAny<TimerCallback>())).Callback<TimerCallback>(c => _timerCallback = c);
			_mockStatusProvider.Setup(s => s.CacheStatus(It.IsAny<GetStatusResult>())).Callback<GetStatusResult>(g => _cachedStatusResult = g);

			_updaterService = new StatusUpdaterService();
		}

		[TestMethod]
		public void ShouldRefreshStatusProviderOnTimerTick() {
			_timerCallback(null);

			_mockStatusProvider.Verify(s => s.Reset());
		}

		[TestMethod]
		public void ShouldGetStrucutreAndDeviceStatusOnTimerTick() {
			_timerCallback(null);

			_mockWebService.Verify(w => w.GetStructureAndDeviceStatusAsync(It.IsAny<Structure>()));
		}

		[TestMethod]
		public void ShouldCacheStatusOnTimerTick() {
			var expectedResult = new GetStatusResult(new[] { new Structure("") });
			_mockWebService.Setup(w => w.GetStructureAndDeviceStatusAsync(It.IsAny<Structure>())).Returns(Task.FromResult(expectedResult));

			_timerCallback(null);

			_mockStatusProvider.Verify(s => s.CacheStatus(expectedResult));
		}

		[TestMethod]
		public void ShouldStopTimerIfGetStructureAndDeviceStatusFails() {
			var expectedResult = new GetStatusResult(WebServiceError.Unknown, new Exception());
			_mockWebService.Setup(w => w.GetStructureAndDeviceStatusAsync(It.IsAny<Structure>())).Returns(Task.FromResult(expectedResult));

			_timerCallback(null);

			_mockTimer.Verify(t => t.Change(Timeout.Infinite, Timeout.Infinite));
		}

		[TestMethod]
		public void ShouldPassCurrentStructureToWebServiceForStatus() {
			var expectedStructure = new Structure("id");
			_updaterService.CurrentStructure = expectedStructure;

			_timerCallback(null);

			_mockWebService.Verify(w=>w.GetStructureAndDeviceStatusAsync(expectedStructure));
		}
	}
}
