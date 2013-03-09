using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class DelayedStatusProviderTest {

		private Mock<INestWebService> _mockWebSerivce;
		private StatusUpdaterService _statusUpdaterService;

		[TestInitialize]
		public void SetUp() {
			_statusUpdaterService = new StatusUpdaterService();
			_mockWebSerivce = new Mock<INestWebService>();
		}

		[TestMethod]
		public void ShouldGetStructureAndThermostatStatusFromWeb() {
			_statusUpdaterService
		}
	}
}
