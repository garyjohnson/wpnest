using System.Collections.Generic;
using System.ComponentModel;

namespace WPNest {

	public class ThermostatSampleViewModel : INotifyPropertyChanged {

		public ThermostatSampleViewModel() {
			CurrentTemperature = "62";
			TargetTemperature = "70";
		}

		private string _currentTemperature = "0";
		public string CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged("CurrentTemperature");
			}
		}
		
		private string _targetTemperature = "0";
		public string TargetTemperature {
			get { return _targetTemperature; }
			set {
				_targetTemperature = value;
				OnPropertyChanged("TargetTemperature");
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

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
