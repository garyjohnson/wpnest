using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPNest {

	internal class ThermostatTickCreator {

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

		private readonly PathGeometry _heavyTicksGeometry = new PathGeometry();
		private readonly PathGeometry _mediumTicksGeometry = new PathGeometry();
		private readonly PathGeometry _lightTicksGeometry = new PathGeometry();

		public PathGeometry HeavyTicksGeometry {
			get { return _heavyTicksGeometry; }
		}

		public PathGeometry MediumTicksGeometry {
			get { return _mediumTicksGeometry; }
		}

		public PathGeometry LightTicksGeometry {
			get { return _lightTicksGeometry; }
		}

		public PathFigure GetPathFigure(Point start, Point end) {
			var tickFigure = new PathFigure();
			tickFigure.StartPoint = start;
			tickFigure.Segments.Add(new LineSegment { Point = end });
			return tickFigure;
		}

		public void RedrawTicks(Size thermostatSize, double currentTemperature, double targetTemperature) {
			ClearTicks();
			DrawMinorTicksForTemperatureTarget(thermostatSize, currentTemperature, targetTemperature);
			DrawTargetTemperatureTick(thermostatSize, targetTemperature);
			DrawCurrentTemperatureTick(thermostatSize, currentTemperature);
		}

		public void RedrawTicksForTemperatureRange(Size thermostatSize, double currentTemperature, double targetTemperatureLow, double targetTemperatureHigh) {
			ClearTicks();
			DrawMinorTicksForTemperatureRange(thermostatSize);
			DrawTargetTemperatureTick(thermostatSize, targetTemperatureLow);
			DrawTargetTemperatureTick(thermostatSize, targetTemperatureHigh);
			DrawCurrentTemperatureTick(thermostatSize, currentTemperature);
		}

		public void ClearTicks() {
			HeavyTicksGeometry.Figures.Clear();
			MediumTicksGeometry.Figures.Clear();
			LightTicksGeometry.Figures.Clear();
		}

		public void UpdateCurrentTemperatureLabelPosition(TextBlock currentTemperatureLabel, Size thermostatSize, double currentTemperature, double targetTemperature) {
			if (currentTemperature == targetTemperature) {
				currentTemperatureLabel.Visibility = Visibility.Collapsed;
				return;
			}

			double temperaturePositionOfLabel = currentTemperature + 1.0d;
			if(currentTemperature < targetTemperature)
				temperaturePositionOfLabel = currentTemperature - 1.0d;

			var rotateTransform = GetRotateTransform(thermostatSize);
			rotateTransform.Angle = AngleFromTemperature(temperaturePositionOfLabel);

			currentTemperatureLabel.Visibility = Visibility.Visible;
			var labelPosition = new Point(thermostatSize.Width / 2, TickMarginFromTop + (TickLength / 2));
			Point rotatedLabelPosition = rotateTransform.Transform(labelPosition);
			double labelHalfHeight = currentTemperatureLabel.ActualHeight / 2;
			double labelHalfWidth = currentTemperatureLabel.ActualWidth / 2;
			Canvas.SetLeft(currentTemperatureLabel, rotatedLabelPosition.X - labelHalfWidth);
			Canvas.SetTop(currentTemperatureLabel, rotatedLabelPosition.Y - labelHalfHeight);
		}

		private void DrawTargetTemperatureTick(Size thermostatSize, double targetTemperature) {
			double halfWidth = thermostatSize.Width / 2;

			var targetStart = new Point(halfWidth, TickMarginFromTop);
			var targetEnd = new Point(halfWidth, TickMarginFromTop + TickTargetTemperatureLength);
			double angle = AngleFromTemperature(targetTemperature);
			var tickTargetFigure = GetRotatedPathFigure(thermostatSize, targetStart, targetEnd, angle);
			HeavyTicksGeometry.Figures.Add(tickTargetFigure);
		}

		private void DrawCurrentTemperatureTick(Size thermostatSize, double currentTemperature) {
			double halfWidth = thermostatSize.Width / 2;

			var currentStart = new Point(halfWidth, TickMarginFromTop);
			var currentEnd = new Point(halfWidth, TickMarginFromTop + TickLength);
			double angle = AngleFromTemperature(currentTemperature);
			var tickCurrentFigure = GetRotatedPathFigure(thermostatSize, currentStart, currentEnd, angle);
			HeavyTicksGeometry.Figures.Add(tickCurrentFigure);
		}

		private PathFigure GetRotatedPathFigure(Size thermostatSize, Point start, Point end, double angle) {
			RotateTransform rotateTransform = GetRotateTransform(thermostatSize);
			rotateTransform.Angle = angle;
			Point rotatedStart = rotateTransform.Transform(start);
			Point rotatedEnd = rotateTransform.Transform(end);
			return GetPathFigure(rotatedStart, rotatedEnd);
		}

		private void DrawMinorTicksForTemperatureRange(Size thermostatSize) {
			double halfWidth = thermostatSize.Width / 2;

			var start = new Point(halfWidth, TickMarginFromTop);
			var end = new Point(halfWidth, TickMarginFromTop + TickLength);

			for (double angle = StartAngle; angle <= EndAngle; angle += TickAngleIncrement) {
				var tickFigure = GetRotatedPathFigure(thermostatSize, start, end, angle);
				LightTicksGeometry.Figures.Add(tickFigure);
			}
		}

		private void DrawMinorTicksForTemperatureTarget(Size thermostatSize, double currentTemperature, double targetTemperature) {
			double halfWidth = thermostatSize.Width / 2;

			var start = new Point(halfWidth, TickMarginFromTop);
			var end = new Point(halfWidth, TickMarginFromTop + TickLength);

			double startTemperature = Math.Min(currentTemperature, targetTemperature);
			double startAngle = AngleFromTemperature(startTemperature);
			double endTemperature = Math.Max(currentTemperature, targetTemperature);
			double endAngle = AngleFromTemperature(endTemperature);

			for (double angle = StartAngle; angle <= EndAngle; angle += TickAngleIncrement) {
				var tickFigure = GetRotatedPathFigure(thermostatSize, start, end, angle);
				if (angle >= startAngle && angle <= endAngle)
					MediumTicksGeometry.Figures.Add(tickFigure);
				else
					LightTicksGeometry.Figures.Add(tickFigure);
			}
		}

		private double AngleFromTemperature(double temperature) {
			return ((temperature - StartDegrees) * AngleDegreeScale) + StartAngle;
		}

		private RotateTransform GetRotateTransform(Size thermostatSize) {
			var rotateTransform = new RotateTransform();
			rotateTransform.CenterX = thermostatSize.Width / 2;
			rotateTransform.CenterY = thermostatSize.Height / 2;
			return rotateTransform;
		}
	}
}
