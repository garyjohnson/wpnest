using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPNest {

	public partial class HvacModeControl : UserControl {

		public HvacModeControl() {
			DataContext = this;
			InitializeComponent();
			Loaded += (sender, args) => RefreshFromHvacMode();
		}

		public static readonly DependencyProperty HvacModeProperty =
			DependencyProperty.Register("HvacMode", typeof(HvacMode), typeof(HvacModeControl), new PropertyMetadata(HvacMode.Off, OnHvacModeChanged));

		private static void OnHvacModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) {
			var self = (HvacModeControl) sender;
			self.RefreshFromHvacMode();
		}

		private void RefreshFromHvacMode() {
			bool isHeatEnabled = HvacMode == HvacMode.HeatAndCool || HvacMode == HvacMode.HeatOnly;
			if (isHeatEnabled)
				HeatDotBrush = (Brush) Resources["HeatDotBrush"];
			else
				HeatDotBrush = (Brush) Resources["DisabledDotBrush"];

			bool isCoolEnabled = HvacMode == HvacMode.HeatAndCool || HvacMode == HvacMode.CoolOnly;
			if (isCoolEnabled)
				CoolDotBrush = (Brush) Resources["CoolDotBrush"];
			else
				CoolDotBrush = (Brush) Resources["DisabledDotBrush"];

			if (HvacMode == HvacMode.Off)
				Label = "OFF";
			if (HvacMode == HvacMode.HeatOnly)
				Label = "HEAT ONLY";
			if (HvacMode == HvacMode.CoolOnly)
				Label = "COOL ONLY";
			if (HvacMode == HvacMode.HeatAndCool)
				Label = "HEAT + COOL";

		}

		public HvacMode HvacMode {
			get { return (HvacMode)GetValue(HvacModeProperty); }
			set { SetValue(HvacModeProperty, value); }
		}

		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register("Label", typeof(string), typeof(HvacModeControl), new PropertyMetadata(""));

		public string Label {
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public static readonly DependencyProperty HeatDotBrushProperty =
			DependencyProperty.Register("HeatDotBrush", typeof(Brush), typeof(HvacModeControl), new PropertyMetadata(null));

		public Brush HeatDotBrush {
			get { return (Brush)GetValue(HeatDotBrushProperty); }
			set { SetValue(HeatDotBrushProperty, value); }
		}

		public static readonly DependencyProperty CoolDotBrushProperty =
			DependencyProperty.Register("CoolDotVisibility", typeof(Brush), typeof(HvacModeControl), new PropertyMetadata(null));

		public Brush CoolDotBrush {
			get { return (Brush)GetValue(CoolDotBrushProperty); }
			set { SetValue(CoolDotBrushProperty, value); }
		}
	}
}
