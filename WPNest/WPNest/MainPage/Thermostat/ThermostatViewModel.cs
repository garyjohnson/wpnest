using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPNest.Annotations;
using WPNest.Services;

namespace WPNest {

	public class ThermostatViewModel : INotifyPropertyChanged {

		public ThermostatViewModel(Thermostat thermostat, Structure structure) {
			TargetTemperature = thermostat.TargetTemperature;
			TargetTemperatureLow = thermostat.TargetTemperatureLow;
			TargetTemperatureHigh = thermostat.TargetTemperatureHigh;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;
			FanMode = thermostat.FanMode;
			IsLeafOn = thermostat.IsLeafOn;
			HvacMode = thermostat.HvacMode;
			IsAway = structure.IsAway;
		}

		private double _targetTemperature;
		public double TargetTemperature {
			get { return _targetTemperature; }
			set {
				_targetTemperature = value;
				OnPropertyChanged();
			}
		}

		private double _targetTemperatureLow;
		public double TargetTemperatureLow {
			get { return _targetTemperatureLow; }
			set {
				_targetTemperatureLow = value;
				OnPropertyChanged();
			}
		}

		private double _targetTemperatureHigh;
		public double TargetTemperatureHigh {
			get { return _targetTemperatureHigh; }
			set {
				_targetTemperatureHigh = value;
				OnPropertyChanged();
			}
		}

		private double _currentTemperature;
		public double CurrentTemperature {
			get { return _currentTemperature; }
			set {
				_currentTemperature = value;
				OnPropertyChanged();
			}
		}

		private bool _isHeating;
		public bool IsHeating {
			get { return _isHeating; }
			set {
				_isHeating = value;
				OnPropertyChanged();
			}
		}


		private bool _isCooling;
		public bool IsCooling {
			get { return _isCooling; }
			set {
				_isCooling = value;
				OnPropertyChanged();
			}
		}

		private bool _isLeafOn;
		public bool IsLeafOn {
			get { return _isLeafOn; }
			set {
				_isLeafOn = value;
				OnPropertyChanged();
			}
		}

		private FanMode _fanMode;
		public FanMode FanMode {
			get { return _fanMode; }
			set {
				if (value != _fanMode) {
					_fanMode = value;
					OnPropertyChanged();
					if(FanModeChanged != null)
						FanModeChanged(this, new FanModeChangedArgs(_fanMode));
				}
			}
		}

		private HvacMode _hvacMode;
		public HvacMode HvacMode {
			get { return _hvacMode; }
			set {
				if (value != _hvacMode) {
					_hvacMode = value;
					OnPropertyChanged();
					if(HvacModeChanged != null)
						HvacModeChanged(this, new HvacModeChangedArgs(_hvacMode));
				}
			}
		}

		private bool _isAway;
		public bool IsAway {
			get { return _isAway; }
			set {
				_isAway = value;
				OnPropertyChanged();
				if(IsAwayChanged != null)
					IsAwayChanged(this, new IsAwayChangedArgs(_isAway));
			}
		}

		public event EventHandler<FanModeChangedArgs> FanModeChanged;
		public event EventHandler<HvacModeChangedArgs> HvacModeChanged;
		public event EventHandler<IsAwayChangedArgs> IsAwayChanged;

		public double GetTemperatureValue(TemperatureMode temperatureMode) {
			double temperatureValue = TargetTemperature;
			if (temperatureMode == TemperatureMode.RangeHigh)
				temperatureValue = TargetTemperatureHigh;
			else if (temperatureMode == TemperatureMode.RangeLow) 
				temperatureValue = TargetTemperatureLow;

			return temperatureValue;
		}

		public void SetTemperatureValue(TemperatureMode temperatureMode, double targetValue) {
			if (temperatureMode == TemperatureMode.RangeHigh)
				TargetTemperatureHigh = targetValue;
			else if (temperatureMode == TemperatureMode.RangeLow)
				TargetTemperatureLow = targetValue;
			else
				TargetTemperature = targetValue;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
