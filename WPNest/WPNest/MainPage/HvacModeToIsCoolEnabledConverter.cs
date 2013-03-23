using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {

	internal class HvacModeToIsCoolEnabledConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is HvacMode) {
				var hvacMode = (HvacMode) value;
				return hvacMode == HvacMode.HeatAndCool || hvacMode == HvacMode.CoolOnly;
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}