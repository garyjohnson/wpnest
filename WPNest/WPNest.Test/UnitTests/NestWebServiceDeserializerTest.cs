using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestWebServiceDeserializerTest {

		private NestWebServiceDeserializer _deserializer;

		[TestInitialize]
		public void SetUp() {
			_deserializer = new NestWebServiceDeserializer();	
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultAwayMode, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResultWhenFalse() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsFalse(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureFromGetStructureStatusResult() {
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, "");

			Assert.IsFalse(structure.IsAway, "Expected Structure.IsAway to be parsed.");
		}

		[TestMethod]
		public void ShouldAssignStructureIdToGetStructureStatusResult() {
			string expectedId = "expectedId";
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, expectedId);

			Assert.AreEqual(expectedId, structure.ID);
		}

		[TestMethod]
		public void ShouldParseLeafFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).Thermostats[0].IsLeafOn);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperature);
		}

		[TestMethod]
		public void ShouldParseCurrentTemperatureFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(22.46d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].CurrentTemperature);
		}

		[TestMethod]
		public void ShouldParseCurrentTemperatureCelciusFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(22.46d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].CurrentTemperature);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureCelciusFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperature);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureLowFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureLowCelciusFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(20d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureHighFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(24d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureHigh);
		}

		[TestMethod]
		public void ShouldParseTargetTemperatureHighCelciusFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			double expectedTemperature = Math.Round(24.444d);
			Assert.AreEqual(expectedTemperature, structures.ElementAt(0).Thermostats[0].TargetTemperatureHigh);
		}

		[TestMethod]
		public void ShouldParseHvacModeFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultTempRangeMode, FakeJsonMessages.UserId);

			Assert.AreEqual(HvacMode.HeatAndCool, structures.ElementAt(0).Thermostats[0].HvacMode);
		}

		[TestMethod]
		public void ShouldParseTemperatureScaleFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultCelcius, FakeJsonMessages.UserId);

			Assert.AreEqual(TemperatureScale.Celcius, structures.ElementAt(0).Thermostats[0].TemperatureScale);
		}

		[TestMethod]
		public void ShouldUpdateHvacModeFromSharedStatusResult() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			Assert.AreEqual(HvacMode.HeatAndCool, thermostat.HvacMode);
		}

		[TestMethod]
		public void ShouldUpdateTargetTemperatureLowFromSharedStatusResult() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			double expectedTemperature = Math.Round(20d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, thermostat.TargetTemperatureLow);
		}

		[TestMethod]
		public void ShouldUpdateTargetTemperatureHighFromSharedStatusResult() {
			var thermostat = new Thermostat("");
			_deserializer.UpdateThermostatStatusFromSharedStatusResult(FakeJsonMessages.GetSharedStatusResultTempRangeMode, thermostat);

			double expectedTemperature = Math.Round(24d.CelciusToFahrenheit());
			Assert.AreEqual(expectedTemperature, thermostat.TargetTemperatureHigh);
		}

		[TestMethod]
		public async Task ShouldParseCancelledErrorFromWebException() {
			var exception = new WebException("Test", WebExceptionStatus.RequestCanceled);

			WebServiceError error = await _deserializer.ParseWebServiceErrorAsync(exception);

			Assert.AreEqual(WebServiceError.Cancelled, error);
		}
	}
}
