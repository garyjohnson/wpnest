using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPNest.Services;

namespace WPNest {

	public class MainPageViewModel : INotifyPropertyChanged {

		private LoginResult _loginResult;
		private GetStatusResult _getStatusResult;

		private string _currentTemperature = "0";
		public string CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged("CurrentTemperature");
			}
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn {
			get { return _isLoggedIn; }
			set { _isLoggedIn = value;
			OnPropertyChanged("IsLoggedIn");}
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

		public async Task LoginAsync() {
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			_loginResult = await nestWebService.LoginAsync(UserName, Password);
			if (IsErrorHandled(_loginResult.Error))
				return;

			_getStatusResult = await nestWebService.GetStatusAsync(_loginResult.TransportUrl, _loginResult.AccessToken, _loginResult.UserId);
			if (IsErrorHandled(_getStatusResult.Error))
				return;

			CurrentTemperature = GetFirstThermostat().Temperature.ToString();
			IsLoggedIn = true;
		}

		private static bool IsErrorHandled(Exception error) {
			if (error != null) {
				MessageBox.Show(error.Message);
				return true;
			}

			return false;
		}

		private Thermostat GetFirstThermostat() {
			return _getStatusResult.Structures.ElementAt(0).Thermostats[0];
		}

		public async Task RaiseTemperatureAsync() {
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			var result = await nestWebService.RaiseTemperatureAsync(_loginResult.TransportUrl, _loginResult.AccessToken, _loginResult.UserId, thermostat);
			if (IsErrorHandled(result.Error))
				return;

			GetTemperatureResult temperatureResult = await nestWebService.GetTemperatureAsync(_loginResult.TransportUrl, _loginResult.AccessToken, _loginResult.UserId, thermostat);
			if (IsErrorHandled(temperatureResult.Error))
				return;

			thermostat.Temperature = temperatureResult.Temperature;
			CurrentTemperature = thermostat.Temperature.ToString(CultureInfo.InvariantCulture);
		}

		public async Task LowerTemperatureAsync() {
			var nestWebService = ServiceContainer.GetService<INestWebService>();
			var thermostat = GetFirstThermostat();

			var result = await nestWebService.LowerTemperatureAsync(_loginResult.TransportUrl, _loginResult.AccessToken, _loginResult.UserId, thermostat);
			if (IsErrorHandled(result.Error))
				return;

			GetTemperatureResult temperatureResult = await nestWebService.GetTemperatureAsync(_loginResult.TransportUrl, _loginResult.AccessToken, _loginResult.UserId, thermostat);
			if (IsErrorHandled(temperatureResult.Error))
				return;

			thermostat.Temperature = temperatureResult.Temperature;
			CurrentTemperature = thermostat.Temperature.ToString(CultureInfo.InvariantCulture);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
