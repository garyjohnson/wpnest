using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {

	public class FanModeToLabelConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is FanMode) {
				var fanMode = (FanMode) value;
				if (fanMode == FanMode.Auto)
					return "Auto";

				return "On";
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
