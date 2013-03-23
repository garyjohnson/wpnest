﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPNest {

	internal class TrueToVisibleConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is bool && (bool)value)
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
