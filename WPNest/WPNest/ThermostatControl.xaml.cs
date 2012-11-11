﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPNest {
	public partial class ThermostatControl : UserControl {
		public ThermostatControl() {
			InitializeComponent();
		}

		private void DrawTicks() {
			PathGeometry ticksGeometry = new PathGeometry();

			var rotateTransform = new RotateTransform();
			rotateTransform.CenterX = orb.ActualWidth / 2;
			rotateTransform.CenterY = orb.ActualHeight / 2;

			Point start = new Point(orb.ActualWidth / 2, 20);
			Point end = new Point(orb.ActualWidth / 2, 50);

			Point newStart;
			Point newEnd;

			for (double i = 140.0d; i > -140.0d; i -= 2.5d) {
				rotateTransform.Angle = i;
				newStart = rotateTransform.Transform(start);
				newEnd = rotateTransform.Transform(end);
				var tickFigure = GetPathFigure(newStart, newEnd);
				ticksGeometry.Figures.Add(tickFigure);
			}

			ticks.Data = ticksGeometry;
		}

		private PathFigure GetPathFigure(Point start, Point end) {
			PathFigure tickFigure = new PathFigure();
			tickFigure.StartPoint = start;

			LineSegment endOfTick = new LineSegment();
			endOfTick.Point = end;

			tickFigure.Segments.Add(endOfTick);
			return tickFigure;
		}

		private void Thermostat_OnLoaded(object sender, RoutedEventArgs e) {
			DrawTicks();
		}

		private async void OnUpClick(object sender, RoutedEventArgs e) {
			await ViewModel.RaiseTemperatureAsync();
		}

		private async void OnDownClick(object sender, RoutedEventArgs e) {
			await ViewModel.LowerTemperatureAsync();
		}

		private MainPageViewModel ViewModel {
			get { return DataContext as MainPageViewModel; }
		}
	}
}
