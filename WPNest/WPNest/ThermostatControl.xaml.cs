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

			BindTargetTemperatureToViewModel();
			BindCurrentTemperatureToViewModel();
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

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private void BindCurrentTemperatureToViewModel() {
			var currentBinding = new Binding("CurrentTemperature");
			currentBinding.Mode = BindingMode.OneWay;
			SetBinding(CurrentTemperatureProperty, currentBinding);
		}

		private void BindTargetTemperatureToViewModel() {
			var binding = new Binding("TargetTemperature");
			binding.Mode = BindingMode.OneWay;
			SetBinding(TargetTemperatureProperty, binding);
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
			UpdateVisualState();
		}

		private async void OnDownClick(object sender, RoutedEventArgs e) {
			await ViewModel.LowerTemperatureAsync();
			UpdateVisualState();
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
			RotateTransform rotateTransform = GetRotateTransform();
			rotateTransform.Angle = AngleFromTemperature(TargetTemperature);

			var targetStart = new Point(GetHalfWidth(), TickMarginFromTop);
			var targetEnd = new Point(GetHalfWidth(), TickMarginFromTop + TickTargetTemperatureLength);
			Point rotatedTargetStart = rotateTransform.Transform(targetStart);
			Point rotatedTargetEnd = rotateTransform.Transform(targetEnd);
			var tickTargetFigure = GetPathFigure(rotatedTargetStart, rotatedTargetEnd);
			_heavyTicksGeometry.Figures.Add(tickTargetFigure);
		}

		private void RefreshCurrentTemperatureLabelPosition() {
			RotateTransform rotateTransform = GetRotateTransform();
			var currentTempLabelPos = new Point(GetHalfWidth(), 35);
			rotateTransform.Angle = AngleFromTemperature(CurrentTemperature);
			Point rotatedLabelPos = rotateTransform.Transform(currentTempLabelPos);

			Canvas.SetLeft(currentTemperature, rotatedLabelPos.X + 10.0d);
			Canvas.SetTop(currentTemperature, rotatedLabelPos.Y - currentTemperature.ActualHeight / 2);
		}

		private void DrawCurrentTemperatureTick() {
			RotateTransform rotateTransform = GetRotateTransform();
			var currentStart = new Point(GetHalfWidth(), TickMarginFromTop);
			var currentEnd = new Point(GetHalfWidth(), TickMarginFromTop + TickLength);
			rotateTransform.Angle = AngleFromTemperature(CurrentTemperature);
			Point rotatedCurrentStart = rotateTransform.Transform(currentStart);
			Point rotatedCurrentEnd = rotateTransform.Transform(currentEnd);
			var tickCurrentFigure = GetPathFigure(rotatedCurrentStart, rotatedCurrentEnd);
			_heavyTicksGeometry.Figures.Add(tickCurrentFigure);
		}

		private void DrawMinorTicks() {
			RotateTransform rotateTransform = GetRotateTransform();

			double halfWidth = orb.ActualWidth / 2;

			var start = new Point(halfWidth, 20);
			var end = new Point(halfWidth, 50);

			double startTemp = Math.Min(CurrentTemperature, TargetTemperature);
			double startAngle = AngleFromTemperature(startTemp);
			double endTemp = Math.Max(CurrentTemperature, TargetTemperature);
			double endAngle = AngleFromTemperature(endTemp);

			for (double i = StartAngle; i <= EndAngle; i += TickAngleIncrement) {
				rotateTransform.Angle = i;
				Point rotatedStart = rotateTransform.Transform(start);
				Point rotatedEnd = rotateTransform.Transform(end);
				var tickFigure = GetPathFigure(rotatedStart, rotatedEnd);

				if (i >= startAngle && i <= endAngle)
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

		private void UserControl_Loaded_1(object sender, RoutedEventArgs e) {

		}

	}
}
