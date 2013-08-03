using System.Collections.Generic;
using System.ComponentModel;

namespace WPNest {

	public class NestSampleViewModel : INotifyPropertyChanged {

		public NestSampleViewModel() {
			CurrentTemperature = 68;
			TargetTemperature = 68;
			FanMode = FanMode.Auto;
			HvacMode = HvacMode.CoolOnly;
			IsLoggedIn = true;
		}

		private bool _isInErrorState = false;
		public bool IsInErrorState {
			get { return _isInErrorState; }
			set {
				_isInErrorState = value;
				OnPropertyChanged("IsInErrorState");
			}
		}

		private double _currentTemperature = 0;
		public double CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged("CurrentTemperature");
			}
		}

		private double _targetTemperature = 0;
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
				_fanMode = value;
				OnPropertyChanged("FanMode");
			}
		}

		private bool _isAway;
		public bool IsAway {
			get { return _isAway; }
			set {
				_isAway = value;
				OnPropertyChanged("IsAway");
			}
		}

		private HvacMode _selectedHvacMode = HvacMode.Off;
		public HvacMode SelectedHvacMode {
			get { return _selectedHvacMode; }
			set {
				_selectedHvacMode = value;
				OnPropertyChanged("SelectedHvacMode");
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

		private HvacMode _hvacMode;
		public HvacMode HvacMode {
			get { return _hvacMode; }
			set {
				_hvacMode = value;
				OnPropertyChanged("HvacMode");
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
