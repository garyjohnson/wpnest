using System;
using System.Globalization;
using System.Windows.Data;
using WPNest.Services;

namespace WPNest.Login {

	public class LoginMessageConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if(value is WebServiceError) {
				var error = (WebServiceError)value;
				if (error == WebServiceError.InvalidCredentials)
					return "Please enter a correct email address and password.";
				if (error == WebServiceError.SessionTokenExpired)
					return "Your session has expired. Please log in again.";

				return "";
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
