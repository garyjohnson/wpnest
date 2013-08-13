﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestViewModelTest {

		public abstract class NestViewModelTestBase {
			internal Mock<IStatusProvider> _statusProvider;
			internal Mock<ISessionProvider> _sessionProvider;
			internal Mock<IAnalyticsService> _analyticsService;
			internal Mock<INestWebService> _nestWebService;
			internal Mock<IStatusUpdaterService> _statusUpdaterService;
			internal NestViewModel _viewModel;
			internal Structure _structure;
			internal Thermostat _firstThermostat;
			internal Thermostat _secondThermostat;

			[TestInitialize]
			public void SetUp() {
				_statusProvider = new Mock<IStatusProvider>();
				_sessionProvider = new Mock<ISessionProvider>();
				_analyticsService = new Mock<IAnalyticsService>();
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
				_nestWebService.Setup(w => w.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>(), It.IsAny<TemperatureMode>())).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>())).Returns(Task.FromResult(new WebServiceResult()));
				_nestWebService.Setup(w => w.SetHvacModeAsync(It.IsAny<Thermostat>(), It.IsAny<HvacMode>())).Returns(Task.FromResult(new WebServiceResult()));
				_statusUpdaterService.Setup(s => s.UpdateStatusAsync()).Returns(Task.Delay(0));

				ServiceContainer.RegisterService<IStatusProvider>(_statusProvider.Object);
				ServiceContainer.RegisterService<ISessionProvider>(_sessionProvider.Object);
				ServiceContainer.RegisterService<IAnalyticsService>(_analyticsService.Object);
				ServiceContainer.RegisterService<INestWebService>(_nestWebService.Object);
				ServiceContainer.RegisterService<IStatusUpdaterService>(_statusUpdaterService.Object);
				_viewModel = new NestViewModel();
			}

			[TestCleanup]
			public void TearDown() {
				_statusProvider = null;
				_sessionProvider = null;
				_analyticsService = null;
				_nestWebService = null;
				_statusUpdaterService = null;
				_viewModel = null;
				_structure = null;
				_firstThermostat = null;
				_secondThermostat = null;
			}
		}

		[TestClass]
		public class NestViewModel_WhenStatusIsUpdated : NestViewModelTestBase {

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
			public void ShouldUpdateTargetTemperatureLow() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {TargetTemperatureLow = 48d};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.TargetTemperatureLow, _viewModel.TargetTemperatureLow, "Expected TargetTemperatureLow to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateTargetTemperatureHigh() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {TargetTemperatureHigh = 48d};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.TargetTemperatureHigh, _viewModel.TargetTemperatureHigh, "Expected TargetTemperatureHigh to update from status change.");
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
			public void ShouldUpdateHvacMode() {
				var structure = new Structure("");
				var thermostat = new Thermostat(""){HvacMode = HvacMode.HeatAndCool};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.HvacMode, _viewModel.HvacMode, "Expected HvacMode to update from status change.");
			}

			[TestMethod]
			public void ShouldUpdateIsLeafOn() {
				var structure = new Structure("");
				var thermostat = new Thermostat("") {IsLeafOn = true};
				structure.Thermostats.Add(thermostat);
				var status = new GetStatusResult(new[] {structure});

				_statusProvider.Raise(provider => provider.StatusUpdated += null, new StatusEventArgs(status));

				Assert.AreEqual(thermostat.IsLeafOn, _viewModel.IsLeafOn, "Expected IsLeafOn to update from status change.");
			}

			[TestMethod]
			public void ShouldNotBeLoggedInOnInvalidCredentialsException() {
				var result = new GetStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.LoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingInOnInvalidCredentialsException() {
				var result = new GetStatusResult(WebServiceError.InvalidCredentials, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(_viewModel.State, NestViewModelState.LoggingIn);
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

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.LoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingInOnSessionTokenExpiredException() {
				var result = new GetStatusResult(WebServiceError.SessionTokenExpired, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(_viewModel.State, NestViewModelState.LoggingIn);
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

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.LoggingIn);
			}

			[TestMethod]
			public void ShouldBeInErrorStateOnServerNotFoundException() {
				var result = new GetStatusResult(WebServiceError.ServerNotFound, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(_viewModel.State, NestViewModelState.Error);
			}

			[TestMethod]
			public void ShouldNotBeInErrorStateOnCanceledException() {
				var result = new GetStatusResult(WebServiceError.Cancelled, new Exception());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.Error);
			}

			[TestMethod]
			public void ShouldBeInErrorStateOnAnyOtherException() {
				var result = new GetStatusResult(WebServiceError.Unknown, new InvalidCastException());
				var args = new StatusEventArgs(result);

				_statusProvider.Raise(provider => provider.StatusUpdated += null, args);

				Assert.AreEqual(_viewModel.State, NestViewModelState.Error);
			}
		}

		[TestClass]
		public class NestViewModel_WhenLoggingOut : NestViewModelTestBase {
			
			[TestMethod]
			public void ShouldStopStatusProvider() {
				_viewModel.LogOut();

				_statusProvider.Verify(s => s.Stop());
			}

			[TestMethod]
			public void ShouldStopStatusUpdater() {
				_viewModel.LogOut();

				_statusUpdaterService.Verify(s => s.Stop());
			}

			[TestMethod]
			public void ShouldNotBeLoggedIn() {
				_viewModel.LogOut();

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.LoggedIn);
			}

			[TestMethod]
			public void ShouldBeLoggingIn() {
				_viewModel.LogOut();

				Assert.AreEqual(_viewModel.State, NestViewModelState.LoggingIn);
			}
		}

		[TestClass]
		public class NestViewModel_WhenLoggingIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldResetCurrentError() {
				_viewModel.CurrentError = WebServiceError.InvalidCredentials;

				await _viewModel.LogInAsync();

				Assert.AreEqual(WebServiceError.None, _viewModel.CurrentError);
			}

			[TestMethod]
			public async Task ShouldClearPasswordField() {
				_viewModel.UserName = "Bob";
				_viewModel.Password = "Bob's Password";

				await _viewModel.LogInAsync();

				Assert.AreEqual(string.Empty, _viewModel.Password);
			}

			[TestMethod]
			public async Task ShouldNotClearUserNameField() {
				_viewModel.UserName = "Bob";
				_viewModel.Password = "Bob's Password";

				await _viewModel.LogInAsync();

				Assert.AreEqual("Bob", _viewModel.UserName);
			}

			[TestMethod]
			public async Task ShouldLoginWithCredentialsIfSessionExpired() {
				string expectedUserName = "Bob";
				string expectedPassword = "Bob's Password";
				_sessionProvider.SetupGet(s => s.IsSessionExpired).Returns(true);
				_viewModel.UserName = expectedUserName;
				_viewModel.Password = expectedPassword;

				await _viewModel.LogInAsync();

				_nestWebService.Verify(n => n.LoginAsync(expectedUserName, expectedPassword));
			}
		}

		[TestClass]
		public class NestViewModel_WhenLoggedIn : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldNotBeLoggingIn() {
				await _viewModel.LogInAsync();

				Assert.AreNotEqual(_viewModel.State, NestViewModelState.LoggingIn);
			}

			[TestMethod]
			public async Task ShouldUpdateTransportUrls() {
				await _viewModel.LogInAsync();

				_nestWebService.Verify(n => n.UpdateTransportUrlAsync());
			}

			[TestMethod]
			public async Task ShouldGetStatus() {
				await _viewModel.LogInAsync();

				_nestWebService.Verify(n => n.GetFullStatusAsync());
			}

			[TestMethod]
			public async Task ShouldBeLoggedIn() {
				await _viewModel.LogInAsync();

				Assert.AreEqual(_viewModel.State, NestViewModelState.LoggedIn);
			}

			[TestMethod]
			public async Task ShouldSetTargetTemperatureToFirstThermostatTargetTemperature() {
				double expectedTargetTemperature = 12.3d;
				_firstThermostat.TargetTemperature = expectedTargetTemperature;

				await _viewModel.LogInAsync();

				Assert.AreEqual(expectedTargetTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldSetCurrentTemperatureToFirstThermostatCurrentTemperature() {
				double expectedCurrentTemperature = 12.3d;
				_firstThermostat.CurrentTemperature = expectedCurrentTemperature;

				await _viewModel.LogInAsync();

				Assert.AreEqual(expectedCurrentTemperature, _viewModel.CurrentTemperature);
			}

			[TestMethod]
			public async Task ShouldSetIsHeatingToFirstThermostatIsHeating() {
				bool expectedIsHeating = true;
				_firstThermostat.IsHeating = expectedIsHeating;

				await _viewModel.LogInAsync();

				Assert.AreEqual(expectedIsHeating, _viewModel.IsHeating);
			}

			[TestMethod]
			public async Task ShouldSetIsCoolingToFirstThermostatIsCooling() {
				bool expectedIsCooling = true;
				_firstThermostat.IsCooling = expectedIsCooling;

				await _viewModel.LogInAsync();

				Assert.AreEqual(expectedIsCooling, _viewModel.IsCooling);
			}

			[TestMethod]
			public async Task ShouldSetFanModeToFirstThermostatFanMode() {
				var expectedFanMode = FanMode.Auto;
				_firstThermostat.FanMode = expectedFanMode;

				await _viewModel.LogInAsync();

				Assert.AreEqual(expectedFanMode, _viewModel.FanMode);
			}

			[TestMethod]
			public async Task ShouldSetIsAwayToStructureIsAway() {
				_structure.IsAway = true;

				await _viewModel.LogInAsync();

				Assert.AreEqual(_structure.IsAway, _viewModel.IsAway);
			}

			[TestMethod]
			public async Task ShouldSetStatusUpdaterCurrentStructureToStructure() {
				await _viewModel.LogInAsync();

				_statusUpdaterService.VerifySet(s => s.CurrentStructure = _structure);
			}

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await _viewModel.LogInAsync();

				_statusUpdaterService.Verify(s => s.Start());
			}

			[TestMethod]
			public async Task ShouldStartStatusProvider() {
				await _viewModel.LogInAsync();

				_statusProvider.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class NestViewModel_WhenTearingDown : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStartStatusUpdater() {
				await _viewModel.LogInAsync();

				_statusUpdaterService.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class NestViewModel_WhenRaisingTemperature : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStopAndStartStatusProvider() {
				await _viewModel.LogInAsync();
				await _viewModel.RaiseTemperatureAsync();

				_statusProvider.Verify(s => s.Stop());
				_statusProvider.Verify(s => s.Start());
			}

			[TestMethod]
			public async Task ShouldIncrementTargetTemperature() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = 31.0d;
				double expectedTemperature = _viewModel.TargetTemperature + 1;
				await _viewModel.RaiseTemperatureAsync();

				Assert.AreEqual(expectedTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldChangedTemperatureOnFirstThermostatToIncrementedTemp() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = 31.0d;
				double expectedTemperature = _viewModel.TargetTemperature + 1;
				await _viewModel.RaiseTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(_firstThermostat, expectedTemperature, TemperatureMode.Target));
			}

			[TestMethod]
			public async Task ShouldNotChangeTemperatureIfTargetTemperatureIsAtMaxiumum() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.RaiseTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>(), It.IsAny<TemperatureMode>()),
					Times.Never(), "Expected ChangeTemperature to not be called.");
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LogInAsync();
				await _viewModel.RaiseTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfChangeTemperatureFails() {
				var result = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>(), It.IsAny<TemperatureMode>()))
					.Returns(Task.FromResult(result));
				await _viewModel.LogInAsync();

				await _viewModel.RaiseTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}
		}

		[TestClass]
		public class NestViewModel_WhenLoweringTemperature : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStopAndStartStatusProvider() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.LowerTemperatureAsync();

				_statusProvider.Verify(s => s.Stop());
				_statusProvider.Verify(s => s.Start());
			}

			[TestMethod]
			public async Task ShouldDecrementTargetTemperature() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				double expectedTemperature = _viewModel.TargetTemperature - 1;
				await _viewModel.LowerTemperatureAsync();

				Assert.AreEqual(expectedTemperature, _viewModel.TargetTemperature);
			}

			[TestMethod]
			public async Task ShouldChangeTemperatureOnFirstThermostatToDecrementedTemp() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				double expectedTemperature = _viewModel.TargetTemperature - 1;
				await _viewModel.LowerTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(_firstThermostat, expectedTemperature, TemperatureMode.Target));
			}

			[TestMethod]
			public async Task ShouldNotChangeTemperatureIfTargetTemperatureIsAtMinimum() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MinTemperature;
				await _viewModel.LowerTemperatureAsync();

				_nestWebService.Verify(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>(), It.IsAny<TemperatureMode>()),
					Times.Never(), "Expected ChangeTemperature to not be called.");
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;
				await _viewModel.LowerTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfChangeTemperatureFails() {
				var result = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.ChangeTemperatureAsync(It.IsAny<Thermostat>(), It.IsAny<double>(), It.IsAny<TemperatureMode>()))
					.Returns(Task.FromResult(result));
				await _viewModel.LogInAsync();
				_viewModel.TargetTemperature = NestViewModel.MaxTemperature;

				await _viewModel.LowerTemperatureAsync();

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}
		}

		[TestClass]
		public class NestViewModel_WhenSettingFanMode : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStopAndStartStatusProvider() {
				await _viewModel.LogInAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusProvider.Verify(s => s.Stop());
				_statusProvider.Verify(s => s.Start());
			}

			[TestMethod]
			public async Task ShouldSetFanModeOnFirstThermostat() {
				await _viewModel.LogInAsync();

				var expectedFanMode = FanMode.Auto;
				_viewModel.FanMode = expectedFanMode;

				Assert.AreEqual(expectedFanMode, _firstThermostat.FanMode);
			}

			[TestMethod]
			public async Task ShouldSetFanModeOnWebService() {
				await _viewModel.LogInAsync();

				_viewModel.FanMode = FanMode.Auto;

				_nestWebService.Verify(n=>n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>()));
			}

			[TestMethod]
			public async Task ShouldUpdateStatus() {
				await _viewModel.LogInAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusUpdaterService.Verify(s=>s.UpdateStatusAsync());
			}

			[TestMethod]
			public async Task ShouldNotUpdateStatusIfSetFanModeFails() {
				var errorResult = new WebServiceResult(WebServiceError.Unknown, new Exception());
				_nestWebService.Setup(n => n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>())).Returns(Task.FromResult(errorResult));
				await _viewModel.LogInAsync();

				_viewModel.FanMode = FanMode.Auto;

				_statusUpdaterService.Verify(s => s.UpdateStatusAsync(), Times.Never());
			}

			[TestMethod]
			public async Task ShouldNotSetFanModeIfFanModeDidNotChange() {
				await _viewModel.LogInAsync();

				_viewModel.FanMode = _viewModel.FanMode;

				_nestWebService.Verify(n => n.SetFanModeAsync(It.IsAny<Thermostat>(), It.IsAny<FanMode>()), 
					Times.Never());
			}
		}

		[TestClass]
		public class NestViewModel_WhenSettingHvacMode : NestViewModelTestBase {

			[TestMethod]
			public async Task ShouldStopAndStartStatusProvider() {
				await _viewModel.LogInAsync();

				_viewModel.HvacMode = HvacMode.HeatAndCool;

				_statusProvider.Verify(s => s.Stop());
				_statusProvider.Verify(s => s.Start());
			}
		}

		[TestClass]
		public class NestViewModel_WhenSettingIsAway : NestViewModelTestBase {
				
			[TestMethod]
			public async Task ShouldStopAndStartStatusProvider() {
				await _viewModel.LogInAsync();

				_viewModel.IsAway = true;

				_statusProvider.Verify(s => s.Stop());
				_statusProvider.Verify(s => s.Start());
			}

			[TestMethod]
			public async Task ShouldCallWebServiceSetAwayMode() {
				await _viewModel.LogInAsync();

				_viewModel.IsAway = true;

				_nestWebService.Verify(w=>w.SetAwayMode(It.IsAny<Structure>(), true));
			}

			[TestMethod]
			public async Task ShouldProvideStructureToWebService() {
				await _viewModel.LogInAsync();

				_viewModel.IsAway = true;

				_nestWebService.Verify(w=>w.SetAwayMode(_structure, It.IsAny<bool>()));
			}

			[TestMethod]
			public async Task ShouldNotSetIsAwayIfSameAsStructure() {
				await _viewModel.LogInAsync();

				_viewModel.IsAway = _structure.IsAway;

				_nestWebService.Verify(n => n.SetAwayMode(It.IsAny<Structure>(), It.IsAny<bool>()), 
					Times.Never());
			}
		}
	}
}
