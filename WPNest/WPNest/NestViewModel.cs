using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPNest.Services;

namespace WPNest {

	public class NestViewModel : INotifyPropertyChanged {

		private const double MinTemperature = 50.0d;
		private const double MaxTemperature = 90.0d;

		private readonly IStatusProvider _statusProvider;
		private readonly ISessionProvider _sessionProvider;
		private readonly INestWebService _nestWebService;
		private readonly StatusUpdaterService _statusUpdater;
		private readonly IAnalyticsService _analyticsService;
		private GetStatusResult _getStatusResult;

		private double _targetTemperature;
		public double TargetTemperature {
			get { return _targetTemperature; }
			set {
				_targetTemperature = value;
				OnPropertyChanged("TargetTemperature");
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
					if(IsLoggedIn)
						SetFanModeAsync(_fanMode);
				}
			}
		}

//		private bool _isAway;
//		public bool IsAway {
//			get { return _isAway; }
//			set {
//				if (value != _isAway) {
//					_isAway = value;
//					OnPropertyChanged("IsAway");
//					if(IsLoggedIn)
//						SetAwayModeAsync(_isAway);
//				}
//			}
//		}

		private WebServiceError _currentError = WebServiceError.None;
		public WebServiceError CurrentError {
			get { return _currentError; }
			set {
				_currentError = value;
				OnPropertyChanged("CurrentError");
			}
		}

//		private HvacMode _selectedHvacMode = HvacMode.Off;
//		public HvacMode SelectedHvacMode {
//			get { return _selectedHvacMode; }
//			set {
//				if (value != _selectedHvacMode) {
//					_selectedHvacMode = value;
//					OnPropertyChanged("SelectedHvacMode");
//					if(IsLoggedIn)
//						SetHvacModeAsync(_selectedHvacMode);
//				}
//			}
//		}

		public NestViewModel() {
			if (DesignerProperties.IsInDesignTool)
				return;

			_statusProvider = ServiceContainer.GetService<IStatusProvider>();
			_sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			_nestWebService = ServiceContainer.GetService<INestWebService>();
			_statusUpdater = ServiceContainer.GetService<StatusUpdaterService>();
			_analyticsService = ServiceContainer.GetService<IAnalyticsService>();
			_statusProvider.ThermostatStatusUpdated += OnThermostatStatusUpdated;
		}

		private void OnThermostatStatusUpdated(object sender, ThermostatStatusEventArgs e) {
			if (IsErrorHandled(e.ThermostatStatus.Error, e.ThermostatStatus.Exception))
				return;

			TargetTemperature = e.ThermostatStatus.TargetTemperature;
			CurrentTemperature = e.ThermostatStatus.CurrentTemperature;
			IsHeating = e.ThermostatStatus.IsHeating;
			IsCooling = e.ThermostatStatus.IsCooling;
//			SelectedHvacMode = e.ThermostatStatus.HvacMode;
			FanMode = e.ThermostatStatus.FanMode;
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

			_getStatusResult = await _nestWebService.GetStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error, _getStatusResult.Exception))
				return;

			IsLoggedIn = true;

//			var structure = GetFirstStructure();
			var thermostat = GetFirstThermostat();
			TargetTemperature = thermostat.TargetTemperature;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;
//			SelectedHvacMode = thermostat.HvacMode;

			_statusUpdater.CurrentThermostat = thermostat;
			_statusUpdater.Start();
		}

		public void Teardown() {
			_statusUpdater.Stop();
		}

		public async Task RaiseTemperatureAsync() {
			if (TargetTemperature >= MaxTemperature)
				return;

			_statusProvider.Reset();

			var thermostat = GetFirstThermostat();

			double desiredTemperature = TargetTemperature + 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			await _statusUpdater.UpdateStatusAsync();
		}

		private async void SetFanModeAsync(FanMode fanMode) {
			var thermostat = GetFirstThermostat();
			if (thermostat.FanMode == fanMode)
				return;

			_statusProvider.Reset();

			thermostat.FanMode = fanMode;
			FanMode = fanMode;
			var result = await _nestWebService.SetFanModeAsync(thermostat, fanMode);
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			await _statusUpdater.UpdateStatusAsync();
		}

//		private async void SetAwayModeAsync(bool isAway) {
//			var structure = GetFirstStructure();
//			if (structure.IsAway == isAway)
//				return;
//
//			_statusProvider.Reset();
//
//			var result = await _nestWebService.SetAwayModeAsync(structure, isAway);
//			if (IsErrorHandled(result.Error, result.Exception))
//				return;
//
//			await _statusUpdater.UpdateStatusAsync();
//		}

//		private async void SetHvacModeAsync(HvacMode hvacMode) {
//			var thermostat = GetFirstThermostat();
//			if (thermostat.HvacMode == hvacMode)
//				return;
//
//			_statusProvider.Reset();
//
//			thermostat.HvacMode = hvacMode;
//			var result = await _nestWebService.SetHvacModeAsync(thermostat, hvacMode);
//			if (IsErrorHandled(result.Error, result.Exception))
//				return;
//
//			await _statusUpdater.UpdateStatusAsync();
//		}

		public async Task LowerTemperatureAsync() {
			if (TargetTemperature <= MinTemperature)
				return;

			_statusProvider.Reset();

			var thermostat = GetFirstThermostat();
			double desiredTemperature = TargetTemperature - 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await _nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			await _statusUpdater.UpdateStatusAsync();
		}

		private Structure GetFirstStructure() {
			return _getStatusResult.Structures.ElementAt(0);
		}

		private Thermostat GetFirstThermostat() {
			return _getStatusResult.Structures.ElementAt(0).Thermostats[0];
		}

		private bool IsErrorHandled(WebServiceError error, Exception exception) {
			if (error == WebServiceError.InvalidCredentials ||
				error == WebServiceError.SessionTokenExpired)
				HandleLoginException(error);
			else if (error == WebServiceError.ServerNotFound)
				HandleException("Server was not found. Please check your network connection and press OK to retry.");
			else if (exception != null)
				HandleException("An unknown error occurred. Press OK to retry.");

			if (exception != null)
				_analyticsService.LogError(exception);

			return exception != null;
		}

		private void HandleException(string message) {
			IsLoggingIn = false;
			MessageBox.Show(message);
			OnLoggedIn();
		}

		private void HandleLoginException(WebServiceError error) {
			IsLoggedIn = false;
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
