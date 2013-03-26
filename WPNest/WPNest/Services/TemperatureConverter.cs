namespace WPNest.Services {

	internal class TemperatureConverter : ITemperatureConverter {

		public double ConvertTo(TemperatureScale toScale, double celciusTemperature) {
			return celciusTemperature;
		}

		public double ConvertFrom(TemperatureScale fromScale, double fromTemperature) {
			return fromTemperature;
		}
	}
}
