using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WPNest.Services;
using WPNest.Annotations;

namespace WPNest {

	public class NestViewModel : INotifyPropertyChanged {

		internal const double MinTemperature = 50.0d;
		internal const double MaxTemperature = 90.0d;

		private readonly IStatusProvider _statusProvider;
		private readonly ISessionProvider _sessionProvider;
		private readonly INestWebService _nestWebService;
		private readonly IStatusUpdaterService _statusUpdater;
		private readonly IAnalyticsService _analyticsService;
		private GetStatusResult _getStatusResult;

		private bool _isInErrorState;
		public bool IsInErrorState {
			get { return _isInErrorState; }
			set {
				_isInErrorState = value;
				OnPropertyChanged();
			}
		}


		private bool _isLoggingIn;
		public bool IsLoggingIn {
			get { return _isLoggingIn; }
			set {
				_isLoggingIn = value;
				OnPropertyChanged();
			}
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn {
			get { return _isLoggedIn; }
			set {
				_isLoggedIn = value;
				OnPropertyChanged();
			}
		}

		private string _userName = "";
		public string UserName {
			get { return _userName; }
			set {
				_userName = value;
				OnPropertyChanged();
			}
		}

		private string _password = "";
		public string Password {
			get { return _password; }
			set {
				_password = value;
				OnPropertyChanged();
			}
		}

		private WebServiceError _currentError = WebServiceError.None;
		public WebServiceError CurrentError {
			get { return _currentError; }
			set {
				_currentError = value;
				OnPropertyChanged();
			}
		}

		public NestViewModel() {
			if (DesignerProperties.IsInDesignTool)
				return;

			_statusProvider = ServiceContainer.GetService<IStatusProvider>();
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_nestWebService = ServiceContainer.GetService<INestWebService>();
			_statusUpdater = ServiceContainer.GetService<IStatusUpdaterService>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
			_statusProvider.StatusUpdated += OnStatusUpdated;
		}

		private void OnStatusUpdated(object sender, StatusEventArgs e) {
			if (IsErrorHandled(e.Status.Error, e.Status.Exception))
				return;

			UpdateViewModelFromGetStatusResult(e.Status);
		}

		private ThermostatViewModel _thermostat;

		private void UpdateViewModelFromGetStatusResult(GetStatusResult statusResult) {
			Structure firstStructure = statusResult.Structures.ElementAt(0);
			Thermostat firstThermostat = firstStructure.Thermostats.ElementAt(0);

			_getStatusResult = statusResult;

			_thermostat = new ThermostatViewModel(firstThermostat, firstStructure);
		}

		public async Task InitializeAsync() {
			if (_sessionProvider.IsSessionExpired) {
				IsLoggingIn = true;
				return;
			}

			await OnLoggedIn();
		}

		public async Task LoginAsync() {
			ResetCurrentError();
			string userName = UserName;
			string password = Password;
			ClearLoginFields();

			if (_sessionProvider.IsSessionExpired) {
				var loginResult = await _nestWebService.LoginAsync(userName, password);
				if (IsErrorHandled(loginResult.Error, loginResult.Exception))
					return;
			}

			await OnLoggedIn();
		}

		private void ResetCurrentError() {
			CurrentError = WebServiceError.None;
		}

		private void ClearLoginFields() {
			Password = string.Empty;
		}

		private async Task OnLoggedIn() {
			IsLoggingIn = false;

			var result = await _nestWebService.UpdateTransportUrlAsync();
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			_getStatusResult = await _nestWebService.GetFullStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error, _getStatusResult.Exception))
				return;

			IsLoggedIn = true;

			UpdateViewModelFromGetStatusResult(_getStatusResult);

			_statusUpdater.CurrentStructure = _getStatusResult.Structures.ElementAt(0);
			_statusUpdater.Start();
		}

		public void Teardown() {
			_statusUpdater.Stop();
		}

		public async Task RaiseLowTemperatureAsync() {
			await RaiseTemperatureAsync(TemperatureMode.RangeLow);
		}

		public async Task RaiseHighTemperatureAsync() {
			await RaiseTemperatureAsync(TemperatureMode.RangeHigh);
		}

		public async Task LowerLowTemperatureAsync() {
			await LowerTemperatureAsync(TemperatureMode.RangeLow);
		}

		public async Task LowerHighTemperatureAsync() {
			await LowerTemperatureAsync(TemperatureMode.RangeHigh);
		}

		public async Task RaiseTemperatureAsync() {
			await RaiseTemperatureAsync(TemperatureMode.Target);
		}


		private void SetThermostatTemperatureValue(TemperatureMode temperatureMode, Thermostat thermostat, double targetValue) {
			if (temperatureMode == TemperatureMode.RangeHigh)
				thermostat.TargetTemperatureHigh = targetValue;
			else if (temperatureMode == TemperatureMode.RangeLow)
				thermostat.TargetTemperatureLow = targetValue;
			else
				thermostat.TargetTemperature = targetValue;
		}

		private async Task RaiseTemperatureAsync(TemperatureMode temperatureMode) {
			double temperature = _thermostat.GetTemperatureValue(temperatureMode);
			if (temperature >= MaxTemperature)
				return;

			try {
				_statusProvider.Stop();

				var thermostat = GetFirstThermostat();

				double desiredTemperature = temperature + 1.0d;
				_thermostat.SetTemperatureValue(temperatureMode, desiredTemperature);
				SetThermostatTemperatureValue(temperatureMode, thermostat, desiredTemperature);

				var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature, temperatureMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			}
			finally {
				_statusProvider.Start();
			}
		}

		public async Task LowerTemperatureAsync() {
			await LowerTemperatureAsync(TemperatureMode.Target);
		}

		public async Task LowerTemperatureAsync(TemperatureMode temperatureMode) {
			double temperature = _thermostat.GetTemperatureValue(temperatureMode);
			if (temperature <= MinTemperature)
				return;

			try {
				_statusProvider.Stop();

				var thermostat = GetFirstThermostat();
				double desiredTemperature = temperature - 1.0d;
				_thermostat.SetTemperatureValue(temperatureMode, desiredTemperature);
				SetThermostatTemperatureValue(temperatureMode, thermostat, desiredTemperature);

				var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature, temperatureMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			}
			finally {
				_statusProvider.Start();
			}
		}

		private async void SetAwayModeAsync(bool isAway) {
			if (!IsLoggedIn)
				return;

			Structure structure = GetFirstStructure();
			if (structure.IsAway == isAway)
				return;

			try {
				_statusProvider.Stop();
				structure.IsAway = isAway;
				await _nestWebService.SetAwayMode(structure, isAway);
			}
			finally {
				_statusProvider.Start();
			}
		}

		private async void SetFanModeAsync(FanMode fanMode) {
			if (!IsLoggedIn)
				return;

			var thermostat = GetFirstThermostat();
			if (thermostat.FanMode == fanMode)
				return;

			try {
				_statusProvider.Stop();

				thermostat.FanMode = fanMode;
				var result = await _nestWebService.SetFanModeAsync(thermostat, fanMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			}
			finally {
				_statusProvider.Start();
			}
		}

		private async void SetHvacModeAsync(HvacMode hvacMode) {
			if (!IsLoggedIn)
				return;

			var thermostat = GetFirstThermostat();
			if (thermostat.HvacMode == hvacMode)
				return;

			try {
				_statusProvider.Stop();

				thermostat.HvacMode = hvacMode;
				var result = await _nestWebService.SetHvacModeAsync(thermostat, hvacMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			}
			finally {
				_statusProvider.Start();
			}
		}

		private Structure GetFirstStructure() {
			return _getStatusResult.Structures.ElementAt(0);
		}

		private Thermostat GetFirstThermostat() {
			return GetFirstStructure().Thermostats[0];
		}

		private bool IsErrorHandled(WebServiceError error, Exception exception) {
			if (error == WebServiceError.InvalidCredentials ||
				error == WebServiceError.SessionTokenExpired)
				HandleLoginException(error);
			else if (error == WebServiceError.Cancelled)
				HandleExceptionByRetry();
			else if (error == WebServiceError.ServerNotFound)
				HandleException();
			else if (exception != null)
				HandleException();

			if (exception != null)
				_analyticsService.LogError(exception);

			return exception != null;
		}

		private void HandleExceptionByRetry() {
			IsLoggingIn = false;
			OnLoggedIn();
		}

		private void HandleException() {
			IsLoggingIn = false;
			IsInErrorState = true;
		}

		public async void RetryAfterErrorAsync() {
			IsInErrorState = false;
			await OnLoggedIn();
		}

		private void HandleLoginException(WebServiceError error) {
			IsLoggedIn = false;
			// Missing test coverage. Set to false before true so UI updates. 
			// How to test this? Or refactor out so not needed?
			IsLoggingIn = false;
			_sessionProvider.ClearSession();
			CurrentError = error;
			IsLoggingIn = true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
