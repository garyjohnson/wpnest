using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {

	public class IsEnabledFanModeConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is FanMode) {
				var fanMode = (FanMode)value;
				return (fanMode == FanMode.Auto || fanMode == FanMode.On);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
