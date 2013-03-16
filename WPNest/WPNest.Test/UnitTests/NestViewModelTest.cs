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
			protected Mock<IStatusProvider> _statusProvider;
			protected Mock<ISessionProvider> _sessionProvider;
			protected Mock<IAnalyticsService> _analyticsService;
			protected Mock<IDialogProvider> _dialogProvider;
			protected Mock<INestWebService> _nestWebService;
			protected Mock<IStatusUpdaterService> _statusUpdaterService;
			protected NestViewModel _viewModel;
			protected Structure _structure;
			protected Thermostat _firstThermostat;
			protected Thermostat _secondThermostat;

			[TestInitialize]
			public void SetUp() {
				_statusProvider = new Mock<IStatusProvider>();
				_sessionProvider = new Mock<ISessionProvider>();
				_analyticsService = new Mock<IAnalyticsService>();
				_dialogProvider = new Mock<IDialogProvider>();
				_nestWebService = new Mock<INestWebService>();
				_statusUpdaterService = new Mock<IStatusUpdaterService>();

				_structure = new Structure("1");
				_firstThermostat = new Thermostat("1");
				_secondThermostat = new Thermostat("1");
				_structure.Thermostats.Add(_firstThermostat);
				_structure.Thermostats.Add(_secondThermostat);
				var structures = new List<Structure> { _structure };

				_nestWebService.Setup(w => w.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.UpdateTransportUrlAsync()).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.SetAwayMode(It.IsAny<Structure>(), It.IsAny<bool>())).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.GetFullStatusAsync()).Returns(Task.FromResult(new GetStatusResult(structures)));
				_nestWebService.Setup(w => w.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>())).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>())).Returns(Task.FromResult(new WebServiceResult()));
				_statusUpdaterService.Setup(s => s.UpdateStatusAsync()).Returns(Task.Delay(0));

				ServiceContainer.RegisterService<IStatusProvider>(_statusProvider.Object);
				ServiceContainer.RegisterService<ISessionProvider>(_sessionProvider.Object);
				ServiceContainer.RegisterService<IAnalyticsService>(_analyticsService.Object);
				ServiceContainer.RegisterService<IDialogProvider>(_dialogProvider.Object);
				ServiceContainer.RegisterService<INestWebService>(_nestWebService.Object);
				ServiceContainer.RegisterService<IStatusUpdaterService>(_statusUpdaterService.Object);
				_viewModel = new NestViewModel();
			}

			[TestCleanup]
			public void TearDown() {
				_statusProvider = null;
				_sessionProvider = null;
				_analyticsService = null;
				_dialogProvider = null;
				_nestWebService = null;
				_statusUpdaterService = null;
				_viewModel = null;
				_structure = null;
				_firstThermostat = null;
				_secondThermostat = null;
			}
		}

		[TestClass]
		public class WhenStatusIsUpdated : NestViewModelTestBase {

			[TestMethod]
			public void ShouldUpdateTargetTemperature() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {TargetTemperature = 48d};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});
				thermostat.TargetTemperature = 48d;

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.TargetTemperature, _viewModel.TargetTemperature, "Expected TargetTemperature to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateCurrentTemperature() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {CurrentTemperature = 37d};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.CurrentTemperature, _viewModel.CurrentTemperature, "Expected CurrentTemperature to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateFanMode() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {FanMode = FanMode.On};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.FanMode, _viewModel.FanMode, "Expected FanMode to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsCooling() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {IsCooling = true};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.IsCooling, _viewModel.IsCooling, "Expected IsCooling to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsHeating() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {IsHeating = true};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.IsHeating, _viewModel.IsHeating, "Expected IsHeating to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsAway() {
				var structure = new Structure("") {IsAway = true};
				var thermostat = new Thermostat("");
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(structure.IsAway, _viewModel.IsAway, "Expected IsAway to update from status change.");
			}

			[TestMethod]
			public void ShouldNotBeLoggedInOnInvalidCredentialsException() {
				var result = new GetStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.IsFalse(_viewModel.IsLoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingInOnInvalidCredentialsException() {
				var result = new GetStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.IsTrue(_viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldSetCurrentErrorToErrorOnInvalidCredentialsException() {
				var expectedError = WebServiceError.InvalidCredentials;
				var result = new GetStatusResult(expectedError, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(expectedError, _viewModel.CurrentError);
			}

			[TestMethod]
			public void ShouldClearSessionOnInvalidCredentialsException() {
				var result = new GetStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				_sessionProvider.Verify(provider => provider.ClearSession(), "Expected session to be cleared when an InvalidCredentials exception occurs.");
			}

			[TestMethod]
			public void ShouldNotBeLoggedInOnSessionTokenExpiredException() {
				var result = new GetStatusResult(WebServiceError.SessionTokenExpired, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.IsFalse(_viewModel.IsLoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingInOnSessionTokenExpiredException() {
				var result = new GetStatusResult(WebServiceError.SessionTokenExpired, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.IsTrue(_viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldSetCurrentErrorToErrorOnSessionTokenExpiredException() {
				var expectedError = WebServiceError.SessionTokenExpired;
				var result = new GetStatusResult(expectedError, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(expectedError, _viewModel.CurrentError);
			}

			[TestMethod]
			public void ShouldLogToAnalyticsOnException() {
				var expectedException = new Exception();
				var result = new GetStatusResult(WebServiceError.Unknown, expectedException);
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				_analyticsService.Verify(analytics => analytics.LogError(expectedException));
			}

			[TestMethod]
			public void ShouldNotBeLoggingInOnServerNotFoundException() {
				var result = new GetStatusResult(WebServiceError.ServerNotFound, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.IsFalse(_viewModel.IsLoggingIn);
			}

			[TestMethod]
			public void ShouldShowMessageOnServerNotFoundException() {
				var result = new GetStatusResult(WebServiceError.ServerNotFound, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				_dialogProvider.Verify(provider => provider.ShowMessageBox(It.IsRegex("Server was not found.")));
			}

			[TestMethod]
			public void ShouldShowMessageOnAnyOtherException() {
				var result = new GetStatusResult(WebServiceError.Unknown, new InvalidCastException());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				_dialogProvider.Verify(provider => provider.ShowMessageBox(It.IsRegex("An unknown error occurred.")));
			}
		}

		[TestClass]
		public class WhenLoggingIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetCurrentError() {
				_viewModel.CurrentError = WebServiceError.InvalidCredentials;

				await _viewModel.LoginAsync();

				Assert.AreEqual(WebServiceError.None, _viewModel.CurrentError);
			}

			[TestMethod]
			public async Task ShouldClearLoginFields() {
				_viewModel.UserName = "Bob";
				_viewModel.Password = "Bob's Password";

				await _viewModel.LoginAsync();

				Assert.AreEqual(string.Empty, _viewModel.UserName);
				Assert.AreEqual(string.Empty, _viewModel.Password);
			}

			[TestMethod]
			public async Task ShouldLoginWithCredentialsIfSessionExpired() {
				string expectedUserName = "Bob";
				string expectedPassword = "Bob's Password";
				_sessionProvider.SetupGet(s => s.IsSessionExpired).Returns(true);
				_viewModel.UserName = expectedUserName;
				_viewModel.Password = expectedPassword;

				await _viewModel.LoginAsync();

				_nestWebService.Verify(n => n.LoginAsync(expectedUserName, expectedPassword));
			}
		}

		[TestClass]
		public class WhenLoggedIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldNotBeLoggingIn() {
				await _viewModel.LoginAsync();

				Assert.IsFalse(_viewModel.IsLoggingIn);
			}

			[TestMethod]
			public async Task ShouldUpdateTransportUrls() {
				await _viewModel.LoginAsync();

				_nestWebService.Verify(n => n.UpdateTransportUrlAsync());
			}

			[TestMethod]
			public async Task ShouldGetStatus() {
				await _viewModel.LoginAsync();

				_nestWebService.Verify(n => n.GetFullStatusAsync());
			}

			[TestMethod]
			public async Task ShouldBeLoggedIn() {
				await _viewModel.LoginAsync();

				Assert.IsTrue(_viewModel.IsLoggedIn);
			}

			[TestMethod]
			public async Task ShouldSetTargetTemperatureToFirstThermostatTargetTemperature() {
				double expectedTargetTemperature = 12.3d;
				_firstThermostat.TargetTemperature = expectedTargetTemperature;

				await _viewModel.LoginAsync();

				Assert.AreEqual(expectedTargetTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldSetCurrentTemperatureToFirstThermostatCurrentTemperature() {
				double expectedCurrentTemperature = 12.3d;
				_firstThermostat.CurrentTemperature = expectedCurrentTemperature;

				await _viewModel.LoginAsync();

				Assert.AreEqual(expectedCurrentTemperature, _viewModel.CurrentTemperature);
			}

			[TestMethod]
			public async Task ShouldSetIsHeatingToFirstThermostatIsHeating() {
				bool expectedIsHeating = true;
				_firstThermostat.IsHeating = expectedIsHeating;

				await _viewModel.LoginAsync();

				Assert.AreEqual(expectedIsHeating, _viewModel.IsHeating);
			}

			[TestMethod]
			public async Task ShouldSetIsCoolingToFirstThermostatIsCooling() {
				bool expectedIsCooling = true;
				_firstThermostat.IsCooling = expectedIsCooling;

				await _viewModel.LoginAsync();

				Assert.AreEqual(expectedIsCooling, _viewModel.IsCooling);
			}

			[TestMethod]
			public async Task ShouldSetFanModeToFirstThermostatFanMode() {
				var expectedFanMode = FanMode.Auto;
				_firstThermostat.FanMode = expectedFanMode;

				await _viewModel.LoginAsync();

				Assert.AreEqual(expectedFanMode, _viewModel.FanMode);
			}

			[TestMethod]
			public async Task ShouldSetIsAwayToStructureIsAway() {
				_structure.IsAway = true;

				await _viewModel.LoginAsync();

				Assert.AreEqual(_structure.IsAway, _viewModel.IsAway);
			}

			[TestMethod]
			public async Task ShouldSetStatusUpdaterCurrentStructureToStructure() {
				await _viewModel.LoginAsync();

				_statusUpdaterService.VerifySet(s => s.CurrentStructure = _structure);
			}

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await _viewModel.LoginAsync();

				_statusUpdaterService.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class WhenTearingDown : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await _viewModel.LoginAsync();

				_statusUpdaterService.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class WhenRaisingTemperature : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetStatusProvider() {
				await _viewModel.LoginAsync();
				await _viewModel.RaiseTemperatureAsync();

				_statusProvider.Verify(s => s.Reset());
			}

			[TestMethod]
			public async Task ShouldIncrementTargetTemperature() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = 31.0d;
				double expectedTemperature = _viewModel.TargetTemperature + 1;
				await _viewModel.RaiseTemperatureAsync();

				Assert.AreEqual(expectedTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldChangedTemperatureOnFirstThermostatToIncrementedTemp() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = 31.0d;
				double expectedTemperature = _viewModel.TargetTemperature + 1;
				await _viewModel.RaiseTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(_firstThermostat, expectedTemperature));
			}

			[TestMethod]
			public async Task ShouldNotChangeTemperatureIfTargetTemperatureIsAtMaxiumum() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.RaiseTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>()),
					Times.Never(), "Expected ChangeTemperature to not be called.");
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LoginAsync();
				await _viewModel.RaiseTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfChangeTemperatureFails() {
				var result = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>()))
					.Returns(Task.FromResult(result));
				await _viewModel.LoginAsync();

				await _viewModel.RaiseTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}
		}

		[TestClass]
		public class WhenLoweringTemperature : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetStatusProvider() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.LowerTemperatureAsync();

				_statusProvider.Verify(s => s.Reset());
			}

			[TestMethod]
			public async Task ShouldDecrementTargetTemperature() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				double expectedTemperature = _viewModel.TargetTemperature - 1;
				await _viewModel.LowerTemperatureAsync();

				Assert.AreEqual(expectedTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldChangeTemperatureOnFirstThermostatToDecrementedTemp() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				double expectedTemperature = _viewModel.TargetTemperature - 1;
				await _viewModel.LowerTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(_firstThermostat, expectedTemperature));
			}

			[TestMethod]
			public async Task ShouldNotChangeTemperatureIfTargetTemperatureIsAtMinimum() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MinTemperature;
				await _viewModel.LowerTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>()),
					Times.Never(), "Expected ChangeTemperature to not be called.");
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.LowerTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfChangeTemperatureFails() {
				var result = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>()))
					.Returns(Task.FromResult(result));
				await _viewModel.LoginAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;

				await _viewModel.LowerTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}
		}

		[TestClass]
		public class WhenSettingFanMode : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetStatusProvider() {
				await _viewModel.LoginAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusProvider.Verify(s => s.Reset());
			}

			[TestMethod]
			public async Task ShouldSetFanModeOnFirstThermostat() {
				await _viewModel.LoginAsync();

				var expectedFanMode = FanMode.Auto;
				_viewModel.FanMode = expectedFanMode;

				Assert.AreEqual(expectedFanMode, _firstThermostat.FanMode);
			}

			[TestMethod]
			public async Task ShouldSetFanModeOnWebService() {
				await _viewModel.LoginAsync();

				_viewModel.FanMode = FanMode.Auto;

				_nestWebService.Verify(n=>n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>()));
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LoginAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusUpdaterService.Verify(s=>s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfSetFanModeFails() {
				var errorResult = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>())).Returns(Task.FromResult(errorResult));
				await _viewModel.LoginAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}

			[TestMethod]
			public async Task ShouldNotSetFanModeIfFanModeDidNotChange() {
				await _viewModel.LoginAsync();

				_viewModel.FanMode = _viewModel.FanMode;

				_nestWebService.Verify(n => n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>()), 
					Times.Never());
			}
		}

		[TestClass]
		public class WhenSettingIsAway : NestViewModelTestBase {
				
			[TestMethod]
			public async Task ShouldResetStatusProvider() {
				await _viewModel.LoginAsync();

				_viewModel.IsAway = true;

				_statusProvider.Verify(s => s.Reset());
			}

			[TestMethod]
			public async Task ShouldCallWebServiceSetAwayMode() {
				await _viewModel.LoginAsync();

				_viewModel.IsAway = true;

				_nestWebService.Verify(w=>w.SetAwayMode(It.IsAny<Structure>(), true));
			}

			[TestMethod]
			public async Task ShouldProvideStructureToWebService() {
				await _viewModel.LoginAsync();

				_viewModel.IsAway = true;

				_nestWebService.Verify(w=>w.SetAwayMode(_structure, It.IsAny<bool>()));
			}

			[TestMethod]
			public async Task ShouldNotSetIsAwayIfSameAsStructure() {
				await _viewModel.LoginAsync();

				_viewModel.IsAway = _structure.IsAway;

				_nestWebService.Verify(n => n.SetAwayMode(It.IsAny<Structure>(), It.IsAny<bool>()), 
					Times.Never());
			}
		}
	}
}
