namespace WPNest.Services {

	internal class TemperatureConverter : ITemperatureConverter {

		public double ConvertTo(TemperatureScale toScale, double celciusTemperature) {
			if (toScale == TemperatureScale.Fahrenheit)
				return celciusTemperature.CelciusToFahrenheit();

			return celciusTemperature;
		}

		public double ConvertFrom(TemperatureScale fromScale, double fromTemperature) {
			if (fromScale == TemperatureScale.Fahrenheit)
				return fromTemperature.FahrenheitToCelcius();

			return fromTemperature;
		}
	}
}
