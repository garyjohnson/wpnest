using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	public class NestWebServiceDeserializerTest {

		internal NestWebServiceDeserializer _deserializer;

		[TestInitialize]
		public void SetUp() {
			_deserializer = new NestWebServiceDeserializer();
		}
	}

	[TestClass]
	public class NestWebServiceDeserializer_WhenParsingGetStatusResult : NestWebServiceDeserializerTest {

		[TestMethod]
		public void ShouldParseStructureIsAway() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultAwayMode, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayWhenFalse() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsFalse(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseLeaf() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).Thermostats[0].IsLeafOn);
		}

		[TestMethod]
		public void ShouldParseTargetTemperature() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperature);
		}

		[TestMethod]
		public void ShouldParseCurrentTemperature() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(22.46d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].CurrentTemperature);
		}

		[TestMethod]
		public void ShouldParseCurrentTemperatureCelcius() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(22.46d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].CurrentTemperature);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureCelcius() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperature);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureLow() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureLowCelcius() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureHigh() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(24d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureHigh);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureHighCelcius() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(24.444d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureHigh);
		}

		[TestMethod]
		public void ShouldParseHvacMode() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultTempRangeMode, FakeJsonMessages.UserId);

			Assert.AreEqual(HvacMode.HeatAndCool, structures.ElementAt(0).Thermostats[0].HvacMode);
		}

		[TestMethod]
		public void ShouldParseTemperatureScale() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			Assert.AreEqual(TemperatureScale.Celcius, structures.ElementAt(0).Thermostats[0].TemperatureScale);
		}
	}

	[TestClass]
	public class NestWebServiceDeserializer_WhenParsingGetSharedStatusResult : NestWebServiceDeserializerTest {

		[TestMethod]
		public void ShouldUpdateHvacMode() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			Assert.AreEqual(HvacMode.HeatAndCool, thermostat.HvacMode);
		}

		[TestMethod]
		public void ShouldUpdateTargetTemperatureLow() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, thermostat.TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldUpdateTargetTemperatureHigh() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			double expectedTemperature = Math.Round(24d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, thermostat.TargetTemperatureHigh);
		}
	}

	[TestClass]
	public class NestWebServiceDeserializer_WhenParsingGetStructureStatusResult : NestWebServiceDeserializerTest {

		[TestMethod]
		public void ShouldParseStructure() {
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, "");

			Assert.IsFalse(structure.IsAway, "Expected Structure.IsAway to be parsed.");
		}

		[TestMethod]
		public void ShouldAssignStructureId() {
			string expectedId = "expectedId";
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, expectedId);

			Assert.AreEqual(expectedId, structure.ID);
		}
	}

	[TestClass]
	public class NestWebServiceDeserializer_WhenParsingWebException : NestWebServiceDeserializerTest {

		[TestMethod]
		public async Task ShouldParseCancelledError() {
			var exception = new WebException("Test", WebExceptionStatus.RequestCanceled);

			WebServiceError error = await _deserializer.ParseWebServiceErrorAsync(exception);

			Assert.AreEqual(WebServiceError.Cancelled, error);
		}
	}
}
