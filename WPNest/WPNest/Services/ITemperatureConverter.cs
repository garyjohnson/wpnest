namespace WPNest.Services {

	internal interface ITemperatureConverter {
		double ConvertTo(TemperatureScale toScale, double celciusTemperature);
		double ConvertFrom(TemperatureScale fromScale, double fromTemperature);
	}
}
