using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestViewModelTest {

		private static Mock<IStatusProvider> statusProvider;
		private static NestViewModel viewModel;

		[TestInitialize]
		public void SetUp() {
			statusProvider = new Mock<IStatusProvider>();

			ServiceContainer.RegisterService<IStatusProvider>(statusProvider.Object);
			viewModel = new NestViewModel();
		}

		[TestClass]
		public class WhenStatusIsUpdated {

			[TestMethod]
			public void ShouldUpdateProperties() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.TargetTemperature = 48d;
				expectedStatus.CurrentTemperature = 37d;
				expectedStatus.FanMode = FanMode.On;
				expectedStatus.IsCooling = true;
				expectedStatus.IsHeating = true;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.TargetTemperature, viewModel.TargetTemperature, "Expected TargetTemperature to update from status change.");
				Assert.AreEqual(expectedStatus.CurrentTemperature, viewModel.CurrentTemperature, "Expected CurrentTemperature to update from status change.");
				Assert.AreEqual(expectedStatus.FanMode, viewModel.FanMode, "Expected FanMode to update from status change.");
				Assert.AreEqual(expectedStatus.IsCooling, viewModel.IsCooling, "Expected IsCooling to update from status change.");
				Assert.AreEqual(expectedStatus.IsHeating, viewModel.IsHeating, "Expected IsHeating to update from status change.");
			}
		}

	}
}
