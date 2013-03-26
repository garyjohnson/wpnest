using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	public class TemperatureConverterTest {

		internal TemperatureConverter _converter;

		[TestInitialize]
		public void SetUp() {
			_converter = new TemperatureConverter();
		}
	}

	[TestClass]
	public class TemperatureConverter_WhenConvertingTo : TemperatureConverterTest {
		
		[TestMethod]
		public void ShouldConvertToCelciusMode() {
			double expectedTemp = 34.0d;

			double actualTemp = _converter.ConvertTo(TemperatureScale.Celcius, expectedTemp);

			Assert.AreEqual(expectedTemp, actualTemp);
		}

		[TestMethod]
		public void ShouldConvertToFahrenheitMode() {
			double celciusTemp = 34.0d;
			double expectedTemp = celciusTemp.CelciusToFahrenheit();

			double actualTemp = _converter.ConvertTo(TemperatureScale.Fahrenheit, celciusTemp);

			Assert.AreEqual(expectedTemp, actualTemp);
		}
	}

	[TestClass]
	public class TemperatureConverter_WhenConvertingFrom : TemperatureConverterTest {
		
		[TestMethod]
		public void ShouldConvertFromCelciusMode() {
			double expectedTemp = 34.0d;

			double actualTemp = _converter.ConvertFrom(TemperatureScale.Celcius, expectedTemp);

			Assert.AreEqual(expectedTemp, actualTemp);
		}

		[TestMethod]
		public void ShouldConvertFromFahrenheitMode() {
			double celciusTemp = 34.0d;
			double fahrenheitTemp = celciusTemp.CelciusToFahrenheit();

			double actualTemp = _converter.ConvertFrom(TemperatureScale.Fahrenheit, fahrenheitTemp);

			Assert.AreEqual(celciusTemp, actualTemp);
		}
	}
}
