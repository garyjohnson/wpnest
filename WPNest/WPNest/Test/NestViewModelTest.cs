using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPNest.Services;

namespace WPNest.Test {

	public class NestViewModelTest {

		[TestClass]
		public class WhenInitialized {

			private NestViewModel _viewModel;

			[TestInitialize]
			public void SetUp() {
				_viewModel = new NestViewModel();
			}

			[TestMethod]
			public void ShouldSubscribeToThermostatStatusUpdates() {
				var expectedResult= new GetThermostatStatusResult(12.3d, 32.1d, true, true);
				var statusProvider = (MockStatusProvider) ServiceContainer.GetService<IStatusProvider>();
				statusProvider.FireThermostatStatusUpdated(expectedResult);

				Assert.AreEqual(expectedResult.TargetTemperature, _viewModel.TargetTemperature);
				Assert.AreEqual(expectedResult.CurrentTemperature, _viewModel.CurrentTemperature);
				Assert.AreEqual(expectedResult.IsHeating, _viewModel.IsHeating);
				Assert.AreEqual(expectedResult.IsCooling, _viewModel.IsCooling);
			}
		}
	}
}
