using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WPNest.Services {

	internal class NestWebServiceDeserializer : INestWebServiceDeserializer {

		public IEnumerable<Structure> ParseStructuresFromGetStatusResult(string responseString, string userId) {
			var structureResults = new List<Structure>();

			var values = JObject.Parse(responseString);
			var structures = values["user"][userId]["structures"];
			foreach (var structure in structures) {
				string structureId = structure.Value<string>().Replace("structure.", "");
				var structureModel = new Structure(structureId);
				structureModel.IsAway = values["structure"][structureId]["away"].Value<bool>();
				structureResults.Add(structureModel);
			}

			int deviceCount = 0;
			foreach (var structureResult in structureResults) {
				var structure = values["structure"][structureResult.ID];
				var devices = structure["devices"];
				foreach (var device in devices) {
					deviceCount++;
					string thermostatId = device.Value<string>().Replace("device.", "");
					var thermostat = new Thermostat(thermostatId);
					structureResult.Thermostats.Add(thermostat);
				}
			}

			foreach (var structureResult in structureResults) {
				foreach (var thermostat in structureResult.Thermostats) {
					var thermostatValues = values["device"][thermostat.ID];
					thermostat.FanMode = GetFanModeFromString(thermostatValues["fan_mode"].Value<string>());
					thermostat.IsLeafOn = thermostatValues["leaf"].Value<bool>();
					TemperatureScale scale = GetTemperatureScaleFromString(thermostatValues["temperature_scale"].Value<string>());
					thermostat.TemperatureScale = scale;
					
					thermostatValues = values["shared"][thermostat.ID];
					double temperature = double.Parse(thermostatValues["target_temperature"].Value<string>());
					thermostat.TargetTemperature = Math.Round(temperature.CelciusToFahrenheit());
					double temperatureLow = double.Parse(thermostatValues["target_temperature_low"].Value<string>());
					thermostat.TargetTemperatureLow = Math.Round(ConvertTo(scale, temperatureLow));
					double temperatureHigh = double.Parse(thermostatValues["target_temperature_high"].Value<string>());
					thermostat.TargetTemperatureHigh = Math.Round(temperatureHigh.CelciusToFahrenheit());
					double currentTemperature = double.Parse(thermostatValues["current_temperature"].Value<string>());
					thermostat.CurrentTemperature = Math.Round(currentTemperature.CelciusToFahrenheit());
					thermostat.IsHeating = thermostatValues["hvac_heater_state"].Value<bool>();
					thermostat.IsCooling = thermostatValues["hvac_ac_state"].Value<bool>();
					thermostat.HvacMode = GetHvacModeFromString(thermostatValues["target_temperature_type"].Value<string>());
				}
			}

			return structureResults;
		}

		private TemperatureScale GetTemperatureScaleFromString(string value) {
			if(value == "F")
				return TemperatureScale.Fahrenheit;
			if(value == "C")
				return TemperatureScale.Celcius;

			throw new InvalidOperationException(string.Format("Could not parse Temperature Scale of {0}", value));
		}

		public Structure ParseStructureFromGetStructureStatusResult(string result, string structureId) {
			var structure = new Structure(structureId);
			var values = JObject.Parse(result);
			structure.IsAway = values["away"].Value<bool>();

			return structure;
		}

		public void UpdateThermostatStatusFromSharedStatusResult(string strContent, Thermostat thermostatToUpdate) {
			var values = JObject.Parse(strContent);
			double temperatureCelcius = double.Parse(values["target_temperature"].Value<string>());
			double temperatureLowCelcius = double.Parse(values["target_temperature_low"].Value<string>());
			double temperatureHighCelcius = double.Parse(values["target_temperature_high"].Value<string>());
			double currentTemperatureCelcius = double.Parse(values["current_temperature"].Value<string>());

			thermostatToUpdate.CurrentTemperature = Math.Round(currentTemperatureCelcius.CelciusToFahrenheit());
			thermostatToUpdate.TargetTemperature = Math.Round(temperatureCelcius.CelciusToFahrenheit());
			thermostatToUpdate.TargetTemperatureLow = Math.Round(temperatureLowCelcius.CelciusToFahrenheit());
			thermostatToUpdate.TargetTemperatureHigh = Math.Round(temperatureHighCelcius.CelciusToFahrenheit());
			thermostatToUpdate.IsHeating = values["hvac_heater_state"].Value<bool>();
			thermostatToUpdate.IsCooling = values["hvac_ac_state"].Value<bool>();
			thermostatToUpdate.HvacMode = GetHvacModeFromString(values["target_temperature_type"].Value<string>());
		}

		public string ParseAccessTokenFromLoginResult(string responseString) {
			var values = JObject.Parse(responseString);
			return values["access_token"].Value<string>();
		}

		public DateTime ParseAccessTokenExpiryFromLoginResult(string responseString) {
			var values = JObject.Parse(responseString);
			return values["expires_in"].Value<DateTime>();
		}

		public string ParseUserIdFromLoginResult(string responseString) {
			var values = JObject.Parse(responseString);
			return values["userid"].Value<string>();
		}

		public string ParseTransportUrlFromResult(string responseString) {
			var values = JObject.Parse(responseString);
			return values["urls"]["transport_url"].Value<string>();
		}

		public FanMode ParseFanModeFromDeviceSubscribeResult(string responseString) {
			var values = JObject.Parse(responseString);
			var fanModeString = values["fan_mode"].Value<string>();
			return GetFanModeFromString(fanModeString);
		}

		private static JObject ParseAsJsonOrNull(string responseString) {
			JObject values = null;
			try {
				values = JObject.Parse(responseString);
			}
			catch (Newtonsoft.Json.JsonException) { }
			return values;
		}

		public string GetHvacModeString(HvacMode hvacMode) {
			if (hvacMode == HvacMode.HeatAndCool)
				return "range";
			if (hvacMode == HvacMode.HeatOnly)
				return "heat";
			if(hvacMode == HvacMode.CoolOnly)
				return "cool";
			if(hvacMode == HvacMode.Off)
				return "off";

			throw new InvalidOperationException();
		}

		private static HvacMode GetHvacModeFromString(string hvacMode) {
			if (hvacMode == "range")
				return HvacMode.HeatAndCool;
			if (hvacMode == "cool")
				return HvacMode.CoolOnly;
			if (hvacMode == "heat")
				return HvacMode.HeatOnly;
			if (hvacMode == "off")
				return HvacMode.Off;

			throw new InvalidOperationException(string.Format("Could not parse Hvac Mode of {0}", hvacMode));
		}

		private static FanMode GetFanModeFromString(string fanMode) {
			if (fanMode == "auto")
				return FanMode.Auto;
			if (fanMode == "on")
				return FanMode.On;

			throw new InvalidOperationException(string.Format("Could not parse Fan Mode of {0}", fanMode));
		}

		public async Task<WebServiceError> ParseWebServiceErrorAsync(Exception exception) {
			var error = WebServiceError.Unknown;

			if (await IsInvalidCredentialsErrorAsync(exception))
				error = WebServiceError.InvalidCredentials;
			else if (IsSessionTokenExpiredError(exception))
				error = WebServiceError.SessionTokenExpired;
			else if (IsNotFoundError(exception))
				error = WebServiceError.ServerNotFound;
			else if(IsCancelledError(exception))
				error = WebServiceError.Cancelled;

			return error;
		}

		private static bool IsCancelledError(Exception exception) {
			bool isCancelledError = false;

			var webException = exception as WebException;
			if (webException != null)
				isCancelledError = webException.Status == WebExceptionStatus.RequestCanceled;

			return isCancelledError;
		}

		public bool ParseLeafFromDeviceSubscribeResult(string responseString) {
			var values = JObject.Parse(responseString);
			return values["leaf"].Value<bool>();
		}

		private static bool IsNotFoundError(Exception exception) {
			HttpStatusCode? statusCode = GetHttpStatusCodeFromException(exception);
			return (statusCode.HasValue && statusCode.Value == HttpStatusCode.NotFound);
		}

		private static bool IsSessionTokenExpiredError(Exception exception) {
			HttpStatusCode? statusCode = GetHttpStatusCodeFromException(exception);
			return (statusCode.HasValue && statusCode.Value == HttpStatusCode.Unauthorized);
		}

		private static async Task<bool> IsInvalidCredentialsErrorAsync(Exception exception) {
			bool isInvalidCredentials = false;
			var webException = exception as WebException;
			if (webException != null && webException.Response != null) {
				string responseString = await webException.Response.GetResponseStringAsync();
				var values = ParseAsJsonOrNull(responseString);
				if (values != null) {
					var errorJson = values["error"];
					if (errorJson != null) {
						var errorMessage = errorJson.Value<string>();
						isInvalidCredentials = errorMessage.Equals("access_denied");
					}
				}
			}

			return isInvalidCredentials;
		}

		private static HttpStatusCode? GetHttpStatusCodeFromException(Exception exception) {
			var webException = exception as WebException;
			if (webException != null && webException.Response != null) {
				var response = (HttpWebResponse)webException.Response;
				return response.StatusCode;
			}
			return null;
		}

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
