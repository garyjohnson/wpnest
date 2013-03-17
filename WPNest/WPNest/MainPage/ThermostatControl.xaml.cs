using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPNest {

	public partial class ThermostatControl : UserControl {

		private const string VisualStateCooling = "Cooling";
		private const string VisualStateHeating = "Heating";
		private const string VisualStateDefault = "Default";

		private const string VisualStateAway = "Away";
		private const string VisualStateTemperatureTarget = "TemperatureTarget";
		private const string VisualStateTemperatureRange = "TemperatureRange";

		private const string VisualStateFanOn = "FanOn";
		private const string VisualStateLeafOn = "LeafOn";
		private const string VisualStateAllOff = "AllOff";

		private readonly ThermostatTickCreator _tickCreator = new ThermostatTickCreator();

		public ThermostatControl() {
			InitializeComponent();
			InitializeBindings();
		}

		public static readonly DependencyProperty TargetTemperatureProperty =
			DependencyProperty.Register("TargetTemperature", typeof(double), typeof(ThermostatControl),
			new PropertyMetadata(0.0d, OnTemperatureChanged));

		public double TargetTemperature {
			get { return (double)GetValue(TargetTemperatureProperty); }
			set { SetValue(TargetTemperatureProperty, value); }
		}

		public static readonly DependencyProperty CurrentTemperatureProperty =
			DependencyProperty.Register("CurrentTemperature", typeof(double), typeof(ThermostatControl),
			new PropertyMetadata(0.0d, OnTemperatureChanged));

		public double CurrentTemperature {
			get { return (double)GetValue(CurrentTemperatureProperty); }
			set { SetValue(CurrentTemperatureProperty, value); }
		}

		public static readonly DependencyProperty IsHeatingProperty =
			DependencyProperty.Register("IsHeating", typeof(bool), typeof(ThermostatControl),
			new PropertyMetadata(false, OnHVACChanged));

		public bool IsHeating {
			get { return (bool)GetValue(IsHeatingProperty); }
			set { SetValue(IsHeatingProperty, value); }
		}

		public static readonly DependencyProperty IsCoolingProperty =
			DependencyProperty.Register("IsCooling", typeof(bool), typeof(ThermostatControl),
			new PropertyMetadata(false, OnHVACChanged));

		public bool IsCooling {
			get { return (bool)GetValue(IsCoolingProperty); }
			set { SetValue(IsCoolingProperty, value); }
		}

		public static readonly DependencyProperty IsAwayProperty =
			DependencyProperty.Register("IsAway", typeof(bool), typeof(ThermostatControl),
			new PropertyMetadata(false, OnIsAwayChanged));

		public bool IsAway {
			get { return (bool)GetValue(IsAwayProperty); }
			set { SetValue(IsAwayProperty, value); }
		}

		public static readonly DependencyProperty FanModeProperty =
			DependencyProperty.Register("FanMode", typeof(FanMode), typeof(ThermostatControl), 
			new PropertyMetadata(FanMode.Auto, OnFanModeChanged));

		public FanMode FanMode {
			get { return (FanMode)GetValue(FanModeProperty); }
			set { SetValue(FanModeProperty, value); }
		}

		public static readonly DependencyProperty IsLeafOnProperty =
			DependencyProperty.Register("IsLeafOn", typeof(bool), typeof(ThermostatControl),
			new PropertyMetadata(false, OnIsLeafOnChanged));

		public bool IsLeafOn {
			get { return (bool)GetValue(IsLeafOnProperty); }
			set { SetValue(IsLeafOnProperty, value); }
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private static void OnIsLeafOnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostat = (ThermostatControl)sender;
			var isLeafOn = (bool)args.NewValue;

			thermostat.UpdateNotificationVisualState(isLeafOn, thermostat.FanMode == FanMode.On);
		}

		private static void OnFanModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostat = (ThermostatControl)sender;
			var fanMode = (FanMode)args.NewValue;

			thermostat.UpdateNotificationVisualState(thermostat.IsLeafOn, fanMode == FanMode.On);
		}

		private static void OnHVACChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostat = (ThermostatControl) sender;
			thermostat.UpdateHvacVisualState();
		}

		private static void OnIsAwayChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostat = (ThermostatControl) sender;
			bool isAway = (bool) args.NewValue;

			thermostat.UpdateAwayVisualState(isAway);
		}

		private static void OnTemperatureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostatControl = (ThermostatControl)sender;
			thermostatControl.RefreshTicks();
		}

		private void OnAwayButtonPressed(object sender, RoutedEventArgs e) {
			MessageBoxResult result = MessageBox.Show("Do you want to end Away Mode?", string.Empty, MessageBoxButton.OKCancel);
			if (result == MessageBoxResult.OK)
				ViewModel.IsAway = false;
		}

		private void OnCurrentTemperatureLayoutUpdated(object sender, EventArgs e) {
			var thermostatSize = new Size(orb.ActualWidth, orb.ActualHeight);
			_tickCreator.UpdateCurrentTemperatureLabelPosition(currentTemperature, thermostatSize, CurrentTemperature, TargetTemperature);
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			heavyTicks.Data = _tickCreator.HeavyTicksGeometry;
			mediumTicks.Data = _tickCreator.MediumTicksGeometry;
			ticks.Data = _tickCreator.LightTicksGeometry;

			RefreshTicks();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			RefreshTicks();
		}

		private void RefreshTicks() {
			var thermostatSize = new Size(orb.ActualWidth, orb.ActualHeight);
			_tickCreator.RedrawTicks(thermostatSize, CurrentTemperature, TargetTemperature);
			_tickCreator.UpdateCurrentTemperatureLabelPosition(currentTemperature, thermostatSize, CurrentTemperature, TargetTemperature);
		}

		private async void OnUpClick(object sender, RoutedEventArgs e) {
			await ViewModel.RaiseTemperatureAsync();
		}

		private async void OnDownClick(object sender, RoutedEventArgs e) {
			await ViewModel.LowerTemperatureAsync();
		}

		private void InitializeBindings() {
			SetBinding(CurrentTemperatureProperty, new Binding("CurrentTemperature"));
			SetBinding(TargetTemperatureProperty, new Binding("TargetTemperature"));
			SetBinding(IsHeatingProperty, new Binding("IsHeating"));
			SetBinding(IsCoolingProperty, new Binding("IsCooling"));
			SetBinding(IsAwayProperty, new Binding("IsAway"));
			SetBinding(FanModeProperty, new Binding("FanMode"));
			SetBinding(IsLeafOnProperty, new Binding("IsLeafOn"));
		}

		private void UpdateHvacVisualState() {
			if (ViewModel.IsCooling)
				GoToVisualState(VisualStateCooling);
			else if (ViewModel.IsHeating)
				GoToVisualState(VisualStateHeating);
			else
				GoToVisualState(VisualStateDefault);
		}

		private void UpdateAwayVisualState(bool isAway) {
			if (isAway)
				GoToVisualState(VisualStateAway);
			else
				GoToVisualState(VisualStateTemperatureTarget);
		}


		private void UpdateNotificationVisualState(bool isLeafOn, bool isFanOn) {
			if(isFanOn)
				GoToVisualState(VisualStateFanOn);
			else if(isLeafOn)
				GoToVisualState(VisualStateLeafOn);
			else
				GoToVisualState(VisualStateAllOff);
		}

		private void GoToVisualState(string visualState) {
			VisualStateManager.GoToState(this, visualState, true);
		}
	}
}
