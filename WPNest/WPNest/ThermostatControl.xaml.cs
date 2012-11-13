using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WPNest {

	public partial class ThermostatControl : UserControl {

		public ThermostatControl() {
			InitializeComponent();

			var binding = new Binding("TargetTemperature");
			binding.Mode = BindingMode.OneWay;
			SetBinding(TargetTemperatureProperty, binding);

			var currentBinding = new Binding("CurrentTemperature");
			currentBinding.Mode = BindingMode.OneWay;
			SetBinding(CurrentTemperatureProperty, currentBinding);
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

		private static void OnTemperatureChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var thermostatControl = (ThermostatControl)sender;
			thermostatControl.DrawMinorTicks();
			thermostatControl.DrawTemperatureTicks();
		}

		private NestViewModel ViewModel {
			get { return DataContext as NestViewModel; }
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			DrawMinorTicks();
			DrawTemperatureTicks();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
			DrawMinorTicks();
			DrawTemperatureTicks();
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
				VisualStateManager.GoToState(this, "Cooling", true);
			else if (ViewModel.IsHeating)
				VisualStateManager.GoToState(this, "Heating", true);
			else
				VisualStateManager.GoToState(this, "Default", true);
		}

		private const double StartAngle = -140.0d;
		private const double EndAngle = 140.0d;
		private const double AngleRange = EndAngle - StartAngle;
		private const double StartDegrees = 50.0d;
		private const double EndDegrees = 90.0d;
		private const double DegreeRange = EndDegrees - StartDegrees;
		private const double TickAngleIncrement = 2.5d;

		private double AngleFromTemperature(double temperature) {
			double scale = AngleRange / DegreeRange;
			return ((temperature - StartDegrees) * scale) + StartAngle;
		}

		private void DrawTemperatureTicks() {
			var heavyTicksGeometry = new PathGeometry();
			double halfWidth = orb.ActualWidth / 2;
			double halfHeight = orb.ActualHeight / 2;
			var rotateTransform = new RotateTransform();
			rotateTransform.CenterX = halfWidth;
			rotateTransform.CenterY = halfHeight;

			Point targetStart = new Point(halfWidth, 20);
			Point targetEnd = new Point(halfWidth, 65);
			rotateTransform.Angle = AngleFromTemperature(TargetTemperature);
			Point rotatedTargetStart = rotateTransform.Transform(targetStart);
			Point rotatedTargetEnd = rotateTransform.Transform(targetEnd);
			var tickTargetFigure = GetPathFigure(rotatedTargetStart, rotatedTargetEnd);
			heavyTicksGeometry.Figures.Add(tickTargetFigure);

			Point currentStart = new Point(halfWidth, 20);
			Point currentEnd = new Point(halfWidth, 50);
			rotateTransform.Angle = AngleFromTemperature(CurrentTemperature);
			Point rotatedCurrentStart = rotateTransform.Transform(currentStart);
			Point rotatedCurrentEnd = rotateTransform.Transform(currentEnd);
			var tickCurrentFigure = GetPathFigure(rotatedCurrentStart, rotatedCurrentEnd);
			heavyTicksGeometry.Figures.Add(tickCurrentFigure);


			Point currentTempLabelPos = new Point(halfWidth, 35);
			rotateTransform.Angle = AngleFromTemperature(CurrentTemperature);
			Point rotatedLabelPos = rotateTransform.Transform(currentTempLabelPos);

			Canvas.SetLeft(currentTemperature, rotatedLabelPos.X + 10.0d);
			Canvas.SetTop(currentTemperature, rotatedLabelPos.Y - currentTemperature.ActualHeight / 2);

			heavyTicks.Data = heavyTicksGeometry;
		}

		private void DrawMinorTicks() {
			var lightTicksGeometry = new PathGeometry();
			var mediumTicksGeometry = new PathGeometry();

			var rotateTransform = new RotateTransform();

			double halfWidth = orb.ActualWidth / 2;
			double halfHeight = orb.ActualHeight / 2;
			rotateTransform.CenterX = halfWidth;
			rotateTransform.CenterY = halfHeight;

			Point start = new Point(halfWidth, 20);
			Point end = new Point(halfWidth, 50);

			double startTemp = Math.Min(CurrentTemperature, TargetTemperature);
			double startAngle = AngleFromTemperature(startTemp);
			double endTemp = Math.Max(CurrentTemperature, TargetTemperature);
			double endAngle = AngleFromTemperature(endTemp);

			for (double i = StartAngle; i <= EndAngle; i += TickAngleIncrement) {
				rotateTransform.Angle = i;
				Point rotatedStart = rotateTransform.Transform(start);
				Point rotatedEnd = rotateTransform.Transform(end);
				var tickFigure = GetPathFigure(rotatedStart, rotatedEnd);

				if (i >= startAngle && i <= endAngle) {
					mediumTicksGeometry.Figures.Add(tickFigure);
				}
				else {
					lightTicksGeometry.Figures.Add(tickFigure);
				}
			}

			ticks.Data = lightTicksGeometry;
			mediumTicks.Data = mediumTicksGeometry;
		}

		private PathFigure GetPathFigure(Point start, Point end) {
			var tickFigure = new PathFigure();
			tickFigure.StartPoint = start;
			tickFigure.Segments.Add(new LineSegment { Point = end });
			return tickFigure;
		}

	}
}
