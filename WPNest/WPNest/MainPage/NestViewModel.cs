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
		private readonly IDialogProvider _dialogProvider;
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
					if (IsLoggedIn)
						SetFanModeAsync(_fanMode);
				}
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
			_statusProvider.ThermostatStatusUpdated += OnThermostatStatusUpdated;
		}

		private void OnThermostatStatusUpdated(object sender, ThermostatStatusEventArgs e) {
			if (IsErrorHandled(e.ThermostatStatus.Error, e.ThermostatStatus.Exception))
				return;

			TargetTemperature = e.ThermostatStatus.Thermostat.TargetTemperature;
			CurrentTemperature = e.ThermostatStatus.Thermostat.CurrentTemperature;
			IsHeating = e.ThermostatStatus.Thermostat.IsHeating;
			IsCooling = e.ThermostatStatus.Thermostat.IsCooling;
			FanMode = e.ThermostatStatus.Thermostat.FanMode;
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

			var thermostat = GetFirstThermostat();
			TargetTemperature = thermostat.TargetTemperature;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;
			FanMode = thermostat.FanMode;

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
