using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPNest.Services;

namespace WPNest {

	public class NestViewModel : INotifyPropertyChanged {

		private GetStatusResult _getStatusResult;
		private readonly Timer _updateStatusTimer;

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

		public NestViewModel() {
			_updateStatusTimer = new Timer(OnTimerTick);
		}

		private void StartUpdateTimer() {
			_updateStatusTimer.Change(0, 5000);
		}

		private void StopUpdateTimer() {
			_updateStatusTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		private async void OnTimerTick(object state) {
			await Deployment.Current.Dispatcher.InvokeAsync(() => {
				UpdateStatusAsync(GetFirstThermostat());
			});
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
			var sessionProvider = ServiceContainer.GetService<ISessionProvider>();
			var nestWebService = ServiceContainer.GetService<INestWebService>();

			if (sessionProvider.IsSessionExpired) {
				var loginResult = await nestWebService.LoginAsync(UserName, Password);
				if (IsErrorHandled(loginResult.Error))
					return;
			}

			await OnLoggedIn();
		}

		private async Task OnLoggedIn() {
			IsLoggingIn = false;
			IsLoggedIn = true;
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			_getStatusResult = await nestWebService.GetStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error))
				return;

			TargetTemperature = GetFirstThermostat().TargetTemperature;
			CurrentTemperature = GetFirstThermostat().CurrentTemperature;
			StartUpdateTimer();
		}

		public async Task RaiseTemperatureAsync() {
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = thermostat.TargetTemperature + 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error))
				return;

			await UpdateStatusAsync(thermostat);
		}

		private Guid latestUpdateStatusRequest;

		private async Task UpdateStatusAsync(Thermostat thermostat) {
			var requestId = new Guid();
			latestUpdateStatusRequest = requestId;

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			GetThermostatStatusResult temperatureResult = await nestWebService.GetThermostatStatusAsync(thermostat);
			if (IsErrorHandled(temperatureResult.Error))
				return;

			if (latestUpdateStatusRequest != requestId)
				return;

			thermostat.TargetTemperature = temperatureResult.TargetTemperature;
			thermostat.CurrentTemperature = temperatureResult.CurrentTemperature;
			thermostat.IsHeating = temperatureResult.IsHeating;
			thermostat.IsCooling = temperatureResult.IsCooling;
			TargetTemperature = thermostat.TargetTemperature;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;
		}

		public async Task LowerTemperatureAsync() {
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = thermostat.TargetTemperature - 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error))
				return;

			await UpdateStatusAsync(thermostat);
		}

		private Thermostat GetFirstThermostat() {
			return _getStatusResult.Structures.ElementAt(0).Thermostats[0];
		}

		private static bool IsErrorHandled(Exception error) {
			if (error != null) {
				MessageBox.Show(error.Message);
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
