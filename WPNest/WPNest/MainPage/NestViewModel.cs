using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WPNest.Services;

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

		private NestViewModelState _state;
		public NestViewModelState State {
			get { return _state; }
			set {
				_state = value;
				OnPropertyChanged("State");
			}
		}

		private bool _isAway;
		public bool IsAway {
			get { return _isAway; }
			set {
				_isAway = value;
				OnPropertyChanged("IsAway");
				if (State == NestViewModelState.LoggedIn)
					SetAwayModeAsync(_isAway);
			}
		}

		private double _targetTemperature;
		public double TargetTemperature {
			get { return _targetTemperature; }
			set {
				_targetTemperature = value;
				OnPropertyChanged("TargetTemperature");
			}
		}

		private double _targetTemperatureLow;
		public double TargetTemperatureLow {
			get { return _targetTemperatureLow; }
			set {
				_targetTemperatureLow = value;
				OnPropertyChanged("TargetTemperatureLow");
			}
		}

		private double _targetTemperatureHigh;
		public double TargetTemperatureHigh {
			get { return _targetTemperatureHigh; }
			set {
				_targetTemperatureHigh = value;
				OnPropertyChanged("TargetTemperatureHigh");
			}
		}

		private double _currentTemperature;
		public double CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged("CurrentTemperature");
			}
		}

		private bool _isHeating;
		public bool IsHeating {
			get { return _isHeating; }
			set {
				_isHeating = value;
				OnPropertyChanged("IsHeating");
			}
		}


		private bool _isCooling;
		public bool IsCooling {
			get { return _isCooling; }
			set {
				_isCooling = value;
				OnPropertyChanged("IsCooling");
			}
		}

		private string _userName = "";
		public string UserName {
			get { return _userName; }
			set {
				_userName = value;
				OnPropertyChanged("UserName");
			}
		}

		private string _password = "";
		public string Password {
			get { return _password; }
			set {
				_password = value;
				OnPropertyChanged("Password");
			}
		}

		private FanMode _fanMode;
		public FanMode FanMode {
			get { return _fanMode; }
			set {
				if (value != _fanMode) {
					_fanMode = value;
					OnPropertyChanged("FanMode");
					if (State == NestViewModelState.LoggedIn)
						SetFanModeAsync(_fanMode);
				}
			}
		}

		private HvacMode _hvacMode;
		public HvacMode HvacMode {
			get { return _hvacMode; }
			set {
				if (value != _hvacMode) {
					_hvacMode = value;
					OnPropertyChanged("HvacMode");
					if (State == NestViewModelState.LoggedIn)
						SetHvacModeAsync(_hvacMode);
				}
			}
		}

		private bool _isLeafOn;
		public bool IsLeafOn {
			get { return _isLeafOn; }
			set {
				_isLeafOn = value;
				OnPropertyChanged("IsLeafOn");
			}
		}

		private WebServiceError _currentError = WebServiceError.None;
		public WebServiceError CurrentError {
			get { return _currentError; }
			set {
				_currentError = value;
				OnPropertyChanged("CurrentError");
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

		private void UpdateViewModelFromGetStatusResult(GetStatusResult statusResult) {
			Structure firstStructure = statusResult.Structures.ElementAt(0);
			Thermostat firstThermostat = firstStructure.Thermostats.ElementAt(0);

			_getStatusResult = statusResult;

			TargetTemperature = firstThermostat.TargetTemperature;
			TargetTemperatureLow = firstThermostat.TargetTemperatureLow;
			TargetTemperatureHigh = firstThermostat.TargetTemperatureHigh;
			CurrentTemperature = firstThermostat.CurrentTemperature;
			IsHeating = firstThermostat.IsHeating;
			IsCooling = firstThermostat.IsCooling;
			FanMode = firstThermostat.FanMode;
			IsLeafOn = firstThermostat.IsLeafOn;
			HvacMode = firstThermostat.HvacMode;
			IsAway = firstStructure.IsAway;
		}

		public Task InitializeAsync() {
			if (_sessionProvider.IsSessionExpired) {
				State = NestViewModelState.LoggingIn;
				return null;
			}

			return OnLoggedIn();
		}

		public async Task LogInAsync() {
			State = NestViewModelState.Loading;

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

		public void LogOut() {
			_statusProvider.Stop();
			_statusUpdater.Stop();
			_sessionProvider.ClearSession();
			State = NestViewModelState.LoggingIn;
		}

		private void ResetCurrentError() {
			CurrentError = WebServiceError.None;
		}

		private void ClearLoginFields() {
			Password = string.Empty;
		}

		private async Task OnLoggedIn() {
			State = NestViewModelState.Loading;

			var result = await _nestWebService.UpdateTransportUrlAsync();
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			_getStatusResult = await _nestWebService.GetFullStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error, _getStatusResult.Exception))
				return;

			State = NestViewModelState.LoggedIn;

			UpdateViewModelFromGetStatusResult(_getStatusResult);

			_statusUpdater.CurrentStructure = _getStatusResult.Structures.ElementAt(0);
			_statusUpdater.Start();
			_statusProvider.Start();
		}

		public void Teardown() {
			_statusUpdater.Stop();
		}

		public Task RaiseLowTemperatureAsync() {
			return RaiseTemperatureAsync(TemperatureMode.RangeLow);
		}

		public Task RaiseHighTemperatureAsync() {
			return RaiseTemperatureAsync(TemperatureMode.RangeHigh);
		}

		public Task LowerLowTemperatureAsync() {
			return LowerTemperatureAsync(TemperatureMode.RangeLow);
		}

		public Task LowerHighTemperatureAsync() {
			return LowerTemperatureAsync(TemperatureMode.RangeHigh);
		}

		public Task RaiseTemperatureAsync() {
			return RaiseTemperatureAsync(TemperatureMode.Target);
		}

		private double GetTemperatureValue(TemperatureMode temperatureMode) {
			double temperatureValue = TargetTemperature;
			if (temperatureMode == TemperatureMode.RangeHigh)
				temperatureValue = TargetTemperatureHigh;
			else if (temperatureMode == TemperatureMode.RangeLow)
				temperatureValue = TargetTemperatureLow;

			return temperatureValue;
		}

		private void SetTemperatureValue(TemperatureMode temperatureMode, double targetValue) {
			if (temperatureMode == TemperatureMode.RangeHigh)
				TargetTemperatureHigh = targetValue;
			else if (temperatureMode == TemperatureMode.RangeLow)
				TargetTemperatureLow = targetValue;
			else
				TargetTemperature = targetValue;
		}

		private void SetThermostatTemperatureValue(TemperatureMode temperatureMode, Thermostat thermostat, double targetValue) {
			if (temperatureMode == TemperatureMode.RangeHigh)
				thermostat.TargetTemperatureHigh = targetValue;
			else if (temperatureMode == TemperatureMode.RangeLow)
				thermostat.TargetTemperatureLow = targetValue;
			else
				thermostat.TargetTemperature = targetValue;
		}

		private Task RaiseTemperatureAsync(TemperatureMode temperatureMode) {
			double temperature = GetTemperatureValue(temperatureMode);
			if (temperature >= MaxTemperature)
				return null;

			return PauseStatusProviderWhile(async () => {
				var thermostat = GetFirstThermostat();

				double desiredTemperature = temperature + 1.0d;
				SetTemperatureValue(temperatureMode, desiredTemperature);
				SetThermostatTemperatureValue(temperatureMode, thermostat, desiredTemperature);

				var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature, temperatureMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			});
		}

		public Task LowerTemperatureAsync() {
			return LowerTemperatureAsync(TemperatureMode.Target);
		}

		public Task LowerTemperatureAsync(TemperatureMode temperatureMode) {
			double temperature = GetTemperatureValue(temperatureMode);
            if (temperature <= MinTemperature)
                return null;

			return PauseStatusProviderWhile(async () => {
				var thermostat = GetFirstThermostat();
				double desiredTemperature = temperature - 1.0d;
				SetTemperatureValue(temperatureMode, desiredTemperature);
				SetThermostatTemperatureValue(temperatureMode, thermostat, desiredTemperature);

				var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature, temperatureMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			});
		}

		private async void SetAwayModeAsync(bool isAway) {
			Structure structure = GetFirstStructure();
			if (structure.IsAway == isAway) {
				return;
			}

			await PauseStatusProviderWhile(async () => {
				structure.IsAway = isAway;
				await _nestWebService.SetAwayMode(structure, isAway);
			});
		}

		private async void SetFanModeAsync(FanMode fanMode) {
			var thermostat = GetFirstThermostat();
			if (thermostat.FanMode == fanMode)
				return;

			await PauseStatusProviderWhile(async () => {
				thermostat.FanMode = fanMode;
				var result = await _nestWebService.SetFanModeAsync(thermostat, fanMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			});
		}

		private async void SetHvacModeAsync(HvacMode hvacMode) {
			var thermostat = GetFirstThermostat();
			if (thermostat.HvacMode == hvacMode)
				return;

			await PauseStatusProviderWhile(async () => {
				thermostat.HvacMode = hvacMode;
				var result = await _nestWebService.SetHvacModeAsync(thermostat, hvacMode);
				if (IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			});
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
			OnLoggedIn();
		}

		private void HandleException() {
			State = NestViewModelState.Error;
		}

		public async void RetryAfterErrorAsync() {
			await OnLoggedIn();
		}

		private void HandleLoginException(WebServiceError error) {
			_sessionProvider.ClearSession();
			CurrentError = error;
			State = NestViewModelState.LoggingIn;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private async Task PauseStatusProviderWhile(Func<Task> action) {
			try {
				_statusProvider.Stop();
				await action();
			}
			finally {
				_statusProvider.Start();
			}
		}
	}
}
