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
	}

	[TestClass]
	public class TemperatureConverter_WhenConvertingFrom : TemperatureConverterTest {
		
	}
}
