using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPNest.Services;

namespace WPNest {

	public class NestViewModel : INotifyPropertyChanged {

		private readonly IStatusProvider _statusProvider;
		private readonly StatusUpdaterService _statusUpdater;
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

		private WebServiceError _currentError = WebServiceError.None;
		public WebServiceError CurrentError {
			get { return _currentError; }
			set {
				_currentError = value;
				OnPropertyChanged("CurrentError");
			}
		}

		public NestViewModel() {
			_statusProvider = ServiceContainer.GetService<IStatusProvider>();
			_statusProvider.ThermostatStatusUpdated += OnThermostatStatusUpdated;
			_statusUpdater = ServiceContainer.GetService<StatusUpdaterService>();
		}

		private void OnThermostatStatusUpdated(object sender, ThermostatStatusEventArgs e) {
			TargetTemperature = e.ThermostatStatus.TargetTemperature;
			CurrentTemperature = e.ThermostatStatus.CurrentTemperature;
			IsHeating = e.ThermostatStatus.IsHeating;
			IsCooling = e.ThermostatStatus.IsCooling;
		}

		public async Task InitializeAsync() {
			var sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			if (sessionProvider.IsSessionExpired) {
				IsLoggingIn = true;
				return;
			}

			await OnLoggedIn();
		}

		public async Task LoginAsync() {
			CurrentError = WebServiceError.None;

			var sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			var nestWebService = ServiceContainer.GetService<INestWebService>();

			if (sessionProvider.IsSessionExpired) {
				var loginResult = await nestWebService.LoginAsync(UserName, Password);
				if (IsErrorHandled(loginResult.Error, loginResult.Exception))
					return;
			}

			await OnLoggedIn();
		}

		private async Task OnLoggedIn() {
			IsLoggingIn = false;
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			_getStatusResult = await nestWebService.GetStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error, _getStatusResult.Exception))
				return;

			IsLoggedIn = true;

			var thermostat = GetFirstThermostat();
			TargetTemperature = thermostat.TargetTemperature;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;

			_statusUpdater.CurrentThermostat = thermostat;
			_statusUpdater.Start();
		}

		public async Task RaiseTemperatureAsync() {
			_statusProvider.Reset();

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = TargetTemperature + 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			await _statusUpdater.UpdateStatusAsync();
		}

		public async Task LowerTemperatureAsync() {
			_statusProvider.Reset();

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = TargetTemperature - 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error, result.Exception))
				return;

			await _statusUpdater.UpdateStatusAsync();
		}

		private Thermostat GetFirstThermostat() {
			return _getStatusResult.Structures.ElementAt(0).Thermostats[0];
		}

		private bool IsErrorHandled(WebServiceError error, Exception exception) {
			if (error == WebServiceError.InvalidCredentials) {
				IsLoggingIn = false;
				var sessionProvider = ServiceContainer.GetService<ISessionProvider>();
				sessionProvider.ClearSession();
				CurrentError = error;
				IsLoggingIn = true;
				return true;
			}
			else if (exception != null) {
				IsLoggingIn = false;
				MessageBox.Show(exception.Message);
				return true;
			}

			return false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
