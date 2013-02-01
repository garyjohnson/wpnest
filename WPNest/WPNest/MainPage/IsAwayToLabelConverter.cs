using System;
using System.Globalization;
using System.Windows.Data;

namespace WPNest {

	public class IsAwayToLabelConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool) {
				if ((bool) value)
					return "Away";

				return "Home";
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
