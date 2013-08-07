using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {



	public class FanModeToBoolConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is FanMode) {
				var fanMode = (FanMode) value;
				return (fanMode == FanMode.Auto);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool) {
				var isAuto = (bool) value;
				if (isAuto)
					return FanMode.Auto;
			}
			
			return FanMode.On;
		}
	}
}
