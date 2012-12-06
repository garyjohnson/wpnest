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

		private GetThermostatStatusResult _cachedThermostatStatus;
		private GetStatusResult _getStatusResult;
		private readonly Timer _updateStatusTimer;
		private readonly Timer _displayCachedStatusTimer;

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
			_displayCachedStatusTimer = new Timer(OnDisplayCachedStatusTick);

		}

		private void OnDisplayCachedStatusTick(object state) {
			Deployment.Current.Dispatcher.InvokeAsync(() => {
				GetThermostatStatusResult cachedStatus = _cachedThermostatStatus;
				if (cachedStatus != null) {
					var thermostat = GetFirstThermostat();
					thermostat.TargetTemperature = cachedStatus.TargetTemperature;
					thermostat.CurrentTemperature = cachedStatus.CurrentTemperature;
					thermostat.IsHeating = cachedStatus.IsHeating;
					thermostat.IsCooling = cachedStatus.IsCooling;
					TargetTemperature = thermostat.TargetTemperature;
					CurrentTemperature = thermostat.CurrentTemperature;
					IsHeating = thermostat.IsHeating;
					IsCooling = thermostat.IsCooling;

					_cachedThermostatStatus = null;
				}
			});
		}

		private void StartUpdateTimer() {
			_updateStatusTimer.Change(2000, 5000);
		}

		private async void OnTimerTick(object state) {
			await UpdateStatusAsync(GetFirstThermostat());
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
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			_getStatusResult = await nestWebService.GetStatusAsync();
			if (IsErrorHandled(_getStatusResult.Error))
				return;

			IsLoggedIn = true;
			TargetTemperature = GetFirstThermostat().TargetTemperature;
			CurrentTemperature = GetFirstThermostat().CurrentTemperature;
			StartUpdateTimer();
			_displayCachedStatusTimer.Change(1000, 1000);
		}

		public async Task RaiseTemperatureAsync() {
			_cachedThermostatStatus = null;

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = TargetTemperature + 1.0d;
			TargetTemperature = desiredTemperature;

			var result = await nestWebService.ChangeTemperatureAsync(thermostat, desiredTemperature);
			if (IsErrorHandled(result.Error))
				return;

			await UpdateStatusAsync(thermostat);
		}

		private async Task UpdateStatusAsync(Thermostat thermostat) {
			_cachedThermostatStatus = null;

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			GetThermostatStatusResult temperatureResult = await nestWebService.GetThermostatStatusAsync(thermostat);
			if (IsErrorHandled(temperatureResult.Error))
				return;

			_cachedThermostatStatus = temperatureResult;
		}


		public async Task LowerTemperatureAsync() {
			_cachedThermostatStatus = null;

			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			double desiredTemperature = TargetTemperature - 1.0d;
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
