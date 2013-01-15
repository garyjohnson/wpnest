using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {
	public class HvacModeToLabelConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is HvacMode) {
				var hvacMode = (HvacMode) value;
				if (hvacMode == HvacMode.Off)
					return "OFF";
				if (hvacMode == HvacMode.CoolOnly)
					return "COOL ONLY";
				if (hvacMode == HvacMode.HeatOnly)
					return "HEAT ONLY";
				if (hvacMode == HvacMode.HeatAndCool)
					return "HEAT + COOL ONLY";
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
