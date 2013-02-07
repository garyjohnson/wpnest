using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestViewModelTest {

		private static Mock<IStatusProvider> statusProvider;
		private static Mock<ISessionProvider> sessionProvider;
		private static Mock<IAnalyticsService> analyticsService;
		private static NestViewModel viewModel;

		private static void TestInitialize() {
			statusProvider = new Mock<IStatusProvider>();
			sessionProvider = new Mock<ISessionProvider>();
			analyticsService = new Mock<IAnalyticsService>();
			ServiceContainer.RegisterService<IStatusProvider>(statusProvider.Object);
			ServiceContainer.RegisterService<ISessionProvider>(sessionProvider.Object);
			ServiceContainer.RegisterService<IAnalyticsService>(analyticsService.Object);
			viewModel = new NestViewModel();
		}

		[TestClass]
		public class WhenStatusIsUpdated {

			[TestInitialize]
			public void SetUp() {
				TestInitialize();
			}

			[TestMethod]
			public void ShouldUpdateTargetTemperature() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.TargetTemperature = 48d;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.TargetTemperature, viewModel.TargetTemperature, "Expected TargetTemperature to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateCurrentTemperature() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.CurrentTemperature = 37d;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.CurrentTemperature, viewModel.CurrentTemperature, "Expected CurrentTemperature to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateFanMode() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.FanMode = FanMode.On;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.FanMode, viewModel.FanMode, "Expected FanMode to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsCooling() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.IsCooling = true;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.IsCooling, viewModel.IsCooling, "Expected IsCooling to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsHeating() {
				var expectedStatus = new GetThermostatStatusResult();
				expectedStatus.IsHeating = true;

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, new ThermostatStatusEventArgs(expectedStatus));

				Assert.AreEqual(expectedStatus.IsHeating, viewModel.IsHeating, "Expected IsHeating to update from status change.");
			}

			[TestMethod]
			public void ShouldNotBeLoggedInOnInvalidCredentialsException() {
				var result = new GetThermostatStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.IsFalse(viewModel.IsLoggedIn);
			}

			[TestMethod]
			public void ShouldBeIsLoggingInOnInvalidCredentialsException() {
				var result = new GetThermostatStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.IsTrue(viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldSetCurrentErrorToErrorOnInvalidCredentialsException() {
				var expectedError = WebServiceError.InvalidCredentials;
				var result = new GetThermostatStatusResult(expectedError, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.AreEqual(expectedError, viewModel.CurrentError);
			}
		}

	}
}
