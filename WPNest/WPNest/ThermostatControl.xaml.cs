using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WPNest {

	public partial class ThermostatControl : UserControl {

		private const double StartAngle = -140.0d;
		private const double EndAngle = 140.0d;
		private const double AngleRange = EndAngle - StartAngle;
		private const double StartDegrees = 50.0d;
		private const double EndDegrees = 90.0d;
		private const double DegreeRange = EndDegrees - StartDegrees;
		private const double TickAngleIncrement = 2.5d;
		private const double AngleDegreeScale = AngleRange / DegreeRange;

		private const double TickMarginFromTop = 20.0d;
		private const double TickLength = 30.0d;
		private const double TickTargetTemperatureLength = 45.0d;

		private const string VisualStateCooling = "Cooling";
		private const string VisualStateHeating = "Heating";
		private const string VisualStateDefault = "Default";

		private readonly PathGeometry _heavyTicksGeometry = new PathGeometry();
		private readonly PathGeometry _mediumTicksGeometry = new PathGeometry();
		private readonly PathGeometry _lightTicksGeometry = new PathGeometry();

		public ThermostatControl() {
			InitializeComponent();

			SetBinding(CurrentTemperatureProperty, new Binding("CurrentTemperature"));
			SetBinding(TargetTemperatureProperty, new Binding("TargetTemperature"));
			SetBinding(IsHeatingProperty, new Binding("IsHeating"));
			SetBinding(IsCoolingProperty, new Binding("IsCooling"));
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

		private static void OnHVACChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostat = (ThermostatControl) sender;
			thermostat.UpdateVisualState();
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private static void OnTemperatureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostatControl = (ThermostatControl)sender;
			thermostatControl.RedrawTicks();
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			heavyTicks.Data = _heavyTicksGeometry;
			mediumTicks.Data = _mediumTicksGeometry;
			ticks.Data = _lightTicksGeometry;

			RedrawTicks();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			RedrawTicks();
		}

		private void RedrawTicks() {
			ClearTicks();
			DrawMinorTicks();
			DrawTargetTemperatureTick();
			DrawCurrentTemperatureTick();
			RefreshCurrentTemperatureLabelPosition();
		}

		private async void OnUpClick(object sender, RoutedEventArgs e) {
			await ViewModel.RaiseTemperatureAsync();
		}

		private async void OnDownClick(object sender, RoutedEventArgs e) {
			await ViewModel.LowerTemperatureAsync();
		}

		private void UpdateVisualState() {
			if (ViewModel.IsCooling)
				VisualStateManager.GoToState(this, VisualStateCooling, true);
			else if (ViewModel.IsHeating)
				VisualStateManager.GoToState(this, VisualStateHeating, true);
			else
				VisualStateManager.GoToState(this, VisualStateDefault, true);
		}

		private double AngleFromTemperature(double temperature) {
			return ((temperature - StartDegrees) * AngleDegreeScale) + StartAngle;
		}

		private RotateTransform GetRotateTransform() {
			var rotateTransform = new RotateTransform();
			rotateTransform.CenterX = GetHalfWidth();
			rotateTransform.CenterY = GetHalfHeight();
			return rotateTransform;
		}

		private double GetHalfHeight() {
			return orb.ActualHeight / 2;
		}

		private double GetHalfWidth() {
			return orb.ActualWidth / 2;
		}

		private void ClearTicks() {
			_heavyTicksGeometry.Figures.Clear();
			_mediumTicksGeometry.Figures.Clear();
			_lightTicksGeometry.Figures.Clear();
		}

		private void DrawTargetTemperatureTick() {
			var targetStart = new Point(GetHalfWidth(), TickMarginFromTop);
			var targetEnd = new Point(GetHalfWidth(), TickMarginFromTop + TickTargetTemperatureLength);
			double angle = AngleFromTemperature(TargetTemperature);
			var tickTargetFigure = GetRotatedPathFigure(targetStart, targetEnd, angle);
			_heavyTicksGeometry.Figures.Add(tickTargetFigure);
		}

		private void RefreshCurrentTemperatureLabelPosition() {
			if (CurrentTemperature == TargetTemperature) {
				currentTemperature.Visibility = Visibility.Collapsed;
				return;
			}

			var rotateTransform = GetRotateTransform();
			rotateTransform.Angle = AngleFromTemperature(CurrentTemperature);
			double labelHalfHeight = currentTemperature.ActualHeight / 2;
			var labelPosition = new Point(GetHalfWidth(), 35 - labelHalfHeight);

			Point rotatedLabelPosition = rotateTransform.Transform(labelPosition);
			Canvas.SetLeft(currentTemperature, rotatedLabelPosition.X + 10.0d);
			Canvas.SetTop(currentTemperature, rotatedLabelPosition.Y);
			currentTemperature.Visibility = Visibility.Visible;
		}

		private void DrawCurrentTemperatureTick() {
			var currentStart = new Point(GetHalfWidth(), TickMarginFromTop);
			var currentEnd = new Point(GetHalfWidth(), TickMarginFromTop + TickLength);
			double angle = AngleFromTemperature(CurrentTemperature);
			var tickCurrentFigure = GetRotatedPathFigure(currentStart, currentEnd, angle);
			_heavyTicksGeometry.Figures.Add(tickCurrentFigure);
		}

		private PathFigure GetRotatedPathFigure(Point start, Point end, double angle) {
			RotateTransform rotateTransform = GetRotateTransform();
			rotateTransform.Angle = angle;
			Point rotatedStart = rotateTransform.Transform(start);
			Point rotatedEnd = rotateTransform.Transform(end);
			return GetPathFigure(rotatedStart, rotatedEnd);
		}

		private void DrawMinorTicks() {
			double halfWidth = orb.ActualWidth / 2;

			var start = new Point(halfWidth, TickMarginFromTop);
			var end = new Point(halfWidth, TickMarginFromTop + TickLength);

			double startTemperature = Math.Min(CurrentTemperature, TargetTemperature);
			double startAngle = AngleFromTemperature(startTemperature);
			double endTemperature = Math.Max(CurrentTemperature, TargetTemperature);
			double endAngle = AngleFromTemperature(endTemperature);

			for (double angle = StartAngle; angle <= EndAngle; angle += TickAngleIncrement) {
				var tickFigure = GetRotatedPathFigure(start, end, angle);
				if (angle >= startAngle && angle <= endAngle)
					_mediumTicksGeometry.Figures.Add(tickFigure);
				else
					_lightTicksGeometry.Figures.Add(tickFigure);
			}
		}

		private PathFigure GetPathFigure(Point start, Point end) {
			var tickFigure = new PathFigure();
			tickFigure.StartPoint = start;
			tickFigure.Segments.Add(new LineSegment { Point = end });
			return tickFigure;
		}
	}
}
