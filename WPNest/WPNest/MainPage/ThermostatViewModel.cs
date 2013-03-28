using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WPNest.Annotations;
using WPNest.Services;

namespace WPNest {

	public class ThermostatViewModel : INotifyPropertyChanged {

		internal const double MinTemperature = 50.0d;
		internal const double MaxTemperature = 90.0d;

		private Thermostat _thermostat;
		private readonly IStatusProvider _statusProvider;
		private readonly INestWebService _nestWebService;
		private readonly IStatusUpdaterService _statusUpdater;
		private readonly IExceptionHandler _exceptionHandler;
		private GetStatusResult _getStatusResult;

		public ThermostatViewModel(Thermostat thermostat) {
			if (DesignerProperties.IsInDesignTool)
				return;

			_thermostat = thermostat;
			_statusProvider = ServiceContainer.GetService<IStatusProvider>();
			_nestWebService = ServiceContainer.GetService<INestWebService>();
			_statusUpdater = ServiceContainer.GetService<IStatusUpdaterService>();
			_exceptionHandler = ServiceContainer.GetService<IExceptionHandler>();
			_statusProvider.StatusUpdated += OnStatusUpdated;
		}

		private void OnStatusUpdated(object sender, StatusEventArgs e) {
			if (_exceptionHandler.IsErrorHandled(e.Status.Error, e.Status.Exception))
				return;

			UpdateViewModelFromGetStatusResult(e.Status);
		}

		private bool _isAway;
		public bool IsAway {
			get { return _isAway; }
			set {
				_isAway = value;
				OnPropertyChanged();
				//				if (IsLoggedIn)
				SetAwayModeAsync(_isAway);
			}
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

		private FanMode _fanMode;
		public FanMode FanMode {
			get { return _fanMode; }
			set {
				if (value != _fanMode) {
					_fanMode = value;
					OnPropertyChanged();
					//					if (IsLoggedIn)
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
					OnPropertyChanged();
					//					if (IsLoggedIn)
					SetHvacModeAsync(_hvacMode);
				}
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
				if (_exceptionHandler.IsErrorHandled(result.Error, result.Exception))
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
				if (_exceptionHandler.IsErrorHandled(result.Error, result.Exception))
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
				if (_exceptionHandler.IsErrorHandled(result.Error, result.Exception))
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
				if (_exceptionHandler.IsErrorHandled(result.Error, result.Exception))
					return;

				await _statusUpdater.UpdateStatusAsync();
			}
			finally {
				_statusProvider.Start();
			}
		}
		private Thermostat GetFirstThermostat() {
			return GetFirstStructure().Thermostats[0];
		}

		private Structure GetFirstStructure() {
			return _getStatusResult.Structures.ElementAt(0);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void UpdateViewModelFromGetStatusResult(GetStatusResult statusResult) {
			Structure firstStructure = statusResult.Structures.ElementAt(0);
			Thermostat thermostat = firstStructure.Thermostats.First(t => t.ID == _thermostat.ID);

			_getStatusResult = statusResult;

			TargetTemperature = thermostat.TargetTemperature;
			TargetTemperatureLow = thermostat.TargetTemperatureLow;
			TargetTemperatureHigh = thermostat.TargetTemperatureHigh;
			CurrentTemperature = thermostat.CurrentTemperature;
			IsHeating = thermostat.IsHeating;
			IsCooling = thermostat.IsCooling;
			FanMode = thermostat.FanMode;
			IsLeafOn = thermostat.IsLeafOn;
			HvacMode = thermostat.HvacMode;
			IsAway = firstStructure.IsAway;
		}
	}
}
