using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestViewModelTest {

		public abstract class NestViewModelTestBase {
			protected Mock<IStatusProvider> statusProvider;
			protected Mock<ISessionProvider> sessionProvider;
			protected Mock<IAnalyticsService> analyticsService;
			protected Mock<IDialogProvider> dialogProvider;
			protected Mock<INestWebService> nestWebService;
			protected Mock<IStatusUpdaterService> statusUpdaterService;
			protected NestViewModel viewModel;
			protected Structure structure;
			protected Thermostat firstThermostat;
			protected Thermostat secondThermostat;

			[TestInitialize]
			public void SetUp() {
				statusProvider = new Mock<IStatusProvider>();
				sessionProvider = new Mock<ISessionProvider>();
				analyticsService = new Mock<IAnalyticsService>();
				dialogProvider = new Mock<IDialogProvider>();
				nestWebService = new Mock<INestWebService>();
				statusUpdaterService = new Mock<IStatusUpdaterService>();

				structure = new Structure("1");
				firstThermostat = new Thermostat("1");
				secondThermostat = new Thermostat("1");
				structure.Thermostats.Add(firstThermostat);
				structure.Thermostats.Add(secondThermostat);
				var structures = new List<Structure> { structure };

				nestWebService.Setup(w => w.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new WebServiceResult()));
				nestWebService.Setup(w => w.UpdateTransportUrlAsync()).Returns(Task.FromResult(new WebServiceResult()));
				nestWebService.Setup(w => w.GetStatusAsync()).Returns(Task.FromResult(new GetStatusResult(structures)));

				ServiceContainer.RegisterService<IStatusProvider>(statusProvider.Object);
				ServiceContainer.RegisterService<ISessionProvider>(sessionProvider.Object);
				ServiceContainer.RegisterService<IAnalyticsService>(analyticsService.Object);
				ServiceContainer.RegisterService<IDialogProvider>(dialogProvider.Object);
				ServiceContainer.RegisterService<INestWebService>(nestWebService.Object);
				ServiceContainer.RegisterService<IStatusUpdaterService>(statusUpdaterService.Object);
				viewModel = new NestViewModel();
			}

			[TestCleanup]
			public void TearDown() {
				statusProvider = null;
				sessionProvider = null;
				analyticsService = null;
				dialogProvider = null;
				nestWebService = null;
				statusUpdaterService = null;
				viewModel = null;
				structure = null;
				firstThermostat = null;
				secondThermostat = null;
			}
		}

		[TestClass]
		public class WhenStatusIsUpdated : NestViewModelTestBase {

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
			public void ShouldBeLoggingInOnInvalidCredentialsException() {
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

			[TestMethod]
			public void ShouldClearSessionOnInvalidCredentialsException() {
				var result = new GetThermostatStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				sessionProvider.Verify(provider => provider.ClearSession(), "Expected session to be cleared when an InvalidCredentials exception occurs.");
			}

			[TestMethod]
			public void ShouldNotBeLoggedInOnSessionTokenExpiredException() {
				var result = new GetThermostatStatusResult(WebServiceError.SessionTokenExpired, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.IsFalse(viewModel.IsLoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingInOnSessionTokenExpiredException() {
				var result = new GetThermostatStatusResult(WebServiceError.SessionTokenExpired, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.IsTrue(viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldSetCurrentErrorToErrorOnSessionTokenExpiredException() {
				var expectedError = WebServiceError.SessionTokenExpired;
				var result = new GetThermostatStatusResult(expectedError, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.AreEqual(expectedError, viewModel.CurrentError);
			}

			[TestMethod]
			public void ShouldLogToAnalyticsOnException() {
				var expectedException = new Exception();
				var result = new GetThermostatStatusResult(WebServiceError.Unknown, expectedException);
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				analyticsService.Verify(analytics => analytics.LogError(expectedException));
			}

			[TestMethod]
			public void ShouldNotBeLoggingInOnServerNotFoundException() {
				var result = new GetThermostatStatusResult(WebServiceError.ServerNotFound, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				Assert.IsFalse(viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldShowMessageOnServerNotFoundException() {
				var result = new GetThermostatStatusResult(WebServiceError.ServerNotFound, new Exception());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				dialogProvider.Verify(provider => provider.ShowMessageBox(It.IsRegex("Server was not found.")));
			}

			[TestMethod]
			public void ShouldShowMessageOnAnyOtherException() {
				var result = new GetThermostatStatusResult(WebServiceError.Unknown, new InvalidCastException());
				var args = new ThermostatStatusEventArgs(result);

				statusProvider.Raise(provider => provider.ThermostatStatusUpdated += null, args);

				dialogProvider.Verify(provider => provider.ShowMessageBox(It.IsRegex("An unknown error occurred.")));
			}
		}

		[TestClass]
		public class WhenLoggingIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetCurrentError() {
				viewModel.CurrentError = WebServiceError.InvalidCredentials;

				await viewModel.LoginAsync();

				Assert.AreEqual(WebServiceError.None, viewModel.CurrentError);
			}

			[TestMethod]
			public async Task ShouldClearLoginFields() {
				viewModel.UserName = "Bob";
				viewModel.Password = "Bob's Password";

				await viewModel.LoginAsync();

				Assert.AreEqual(string.Empty, viewModel.UserName);
				Assert.AreEqual(string.Empty, viewModel.Password);
			}

			[TestMethod]
			public async Task ShouldLoginWithCredentialsIfSessionExpired() {
				string expectedUserName = "Bob";
				string expectedPassword = "Bob's Password";
				sessionProvider.SetupGet(s => s.IsSessionExpired).Returns(true);
				viewModel.UserName = expectedUserName;
				viewModel.Password = expectedPassword;

				await viewModel.LoginAsync();

				nestWebService.Verify(n => n.LoginAsync(expectedUserName, expectedPassword));
			}
		}

		[TestClass]
		public class WhenLoggedIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldNotBeLoggingIn() {
				await viewModel.LoginAsync();

				Assert.IsFalse(viewModel.IsLoggingIn);
			}

			[TestMethod]
			public async Task ShouldUpdateTransportUrls() {
				await viewModel.LoginAsync();

				nestWebService.Verify(n => n.UpdateTransportUrlAsync());
			}

			[TestMethod]
			public async Task ShouldGetStatus() {
				await viewModel.LoginAsync();

				nestWebService.Verify(n => n.GetStatusAsync());
			}

			[TestMethod]
			public async Task ShouldBeLoggedIn() {
				await viewModel.LoginAsync();

				Assert.IsTrue(viewModel.IsLoggedIn);
			}

			[TestMethod]
			public async Task ShouldSetTargetTemperatureToFirstThermostatTargetTemperature() {
				double expectedTargetTemperature = 12.3d;
				firstThermostat.TargetTemperature = expectedTargetTemperature;

				await viewModel.LoginAsync();

				Assert.AreEqual(expectedTargetTemperature, viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldSetCurrentTemperatureToFirstThermostatCurrentTemperature() {
				double expectedCurrentTemperature = 12.3d;
				firstThermostat.CurrentTemperature = expectedCurrentTemperature;

				await viewModel.LoginAsync();

				Assert.AreEqual(expectedCurrentTemperature, viewModel.CurrentTemperature);
			}

			[TestMethod]
			public async Task ShouldSetIsHeatingToFirstThermostatIsHeating() {
				bool expectedIsHeating = true;
				firstThermostat.IsHeating = expectedIsHeating;

				await viewModel.LoginAsync();

				Assert.AreEqual(expectedIsHeating, viewModel.IsHeating);
			}

			[TestMethod]
			public async Task ShouldSetIsCoolingToFirstThermostatIsCooling() {
				bool expectedIsCooling = true;
				firstThermostat.IsCooling = expectedIsCooling;

				await viewModel.LoginAsync();

				Assert.AreEqual(expectedIsCooling, viewModel.IsCooling);
			}

			[TestMethod]
			public async Task ShouldSetFanModeToFirstThermostatFanMode() {
				var expectedFanMode = FanMode.On;
				firstThermostat.FanMode = expectedFanMode;

				await viewModel.LoginAsync();

				Assert.AreEqual(expectedFanMode, viewModel.FanMode);
			}

			[TestMethod]
			public async Task ShouldSetStatusUpdaterCurrentThermostatToFirstThermostat() {
				await viewModel.LoginAsync();

				statusUpdaterService.VerifySet(s => s.CurrentThermostat = firstThermostat);
			}

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await viewModel.LoginAsync();

				statusUpdaterService.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class WhenTearingDown : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await viewModel.LoginAsync();

				statusUpdaterService.Verify(s => s.Start());
			}
		}
	}
}
