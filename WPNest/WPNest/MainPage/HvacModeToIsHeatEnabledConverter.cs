using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {

	public class HvacModeToIsHeatEnabledConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is HvacMode) {
				var hvacMode = (HvacMode) value;
				return hvacMode == HvacMode.HeatAndCool || hvacMode == HvacMode.HeatOnly;
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
