using System;
using System.ComponentModel;
using System.Diagnostics;
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
		private readonly IDialogProvider _dialogProvider;
		private GetStatusResult _getStatusResult;

		private bool _isAway;
		public bool IsAway {
			get { return _isAway; }
			set {
				_isAway = value;
				OnPropertyChanged("IsAway");
				if (IsLoggedIn)
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

		private bool _isLoggingIn;
		public bool IsLoggingIn {
			get { return _isLoggingIn; }
			set {
				_isLoggingIn = value;
				OnPropertyChanged("IsLoggingIn");
			}
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn {
			get { return _isLoggedIn; }
			set {
				_isLoggedIn = value;
				OnPropertyChanged("IsLoggedIn");
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
					if (IsLoggedIn)
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
					if (IsLoggedIn)
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
			_dialogProvider = ServiceContainer.GetService<IDialogProvider>();
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
			UserName = string.Empty;
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

		private async Task RaiseTemperatureAsync(TemperatureMode temperatureMode) {
			double temperature = GetTemperatureValue(temperatureMode);
			if (temperature >= MaxTemperature)
				return;

			try {
				_statusProvider.Stop();

				var thermostat = GetFirstThermostat();

				double desiredTemperature = temperature + 1.0d;
				SetTemperatureValue(temperatureMode, desiredTemperature);
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
			double temperature = GetTemperatureValue(temperatureMode);
			if (temperature <= MinTemperature)
				return;

			try {
				_statusProvider.Stop();

				var thermostat = GetFirstThermostat();
				double desiredTemperature = temperature - 1.0d;
				SetTemperatureValue(temperatureMode, desiredTemperature);
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
			Structure structure = GetFirstStructure();
			if (structure.IsAway == isAway) {
				return;
			}

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
				HandleException();
			else if (error == WebServiceError.ServerNotFound)
				HandleException("Server was not found. Please check your network connection and press OK to retry.");
			else if (exception != null)
				HandleException("An unknown error occurred. Press OK to retry.");

			if (exception != null)
				_analyticsService.LogError(exception);

			return exception != null;
		}

		private void HandleException(string message = null) {
			IsLoggingIn = false;
			if (message != null)
				_dialogProvider.ShowMessageBox(message);
			OnLoggedIn();
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

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
