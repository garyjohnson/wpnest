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

		[TestInitialize]
		public void SetUp() {
			_mockTimer = new Mock<ITimer>();
			_mockStatusProvider = new Mock<IStatusProvider>();
			_mockWebService = new Mock<INestWebService>();

			ServiceContainer.RegisterService<ITimer>(_mockTimer.Object);
			ServiceContainer.RegisterService<INestWebService>(_mockWebService.Object);
			ServiceContainer.RegisterService<IStatusProvider>(_mockStatusProvider.Object);

			_mockWebService.Setup(w => w.GetThermostatStatusAsync(It.IsAny<Thermostat>())).Returns(Task.FromResult(new GetThermostatStatusResult(new Thermostat(""))));

			_updaterService = new StatusUpdaterService();
		}

		[TestMethod]
		public void ShouldRefreshStatusProviderOnTimerTick() {
			TimerCallback callback = null;
			_mockTimer.Setup(t => t.SetCallback(It.IsAny<TimerCallback>())).Callback<TimerCallback>(c => callback = c);

			_updaterService = new StatusUpdaterService();
			callback(null);

			_mockStatusProvider.Verify(s=>s.Reset());
		}

		[TestMethod]
		public void ShouldGetStrucutreAndDeviceStatusOnTimerTick() {
			TimerCallback callback = null;
			_mockTimer.Setup(t => t.SetCallback(It.IsAny<TimerCallback>())).Callback<TimerCallback>(c => callback = c);

			_updaterService = new StatusUpdaterService();
			callback(null);

			_mockWebService.Verify(w=>w.GetStructureAndDeviceStatusAsync(It.IsAny<Structure>()));
		}
	}
}
