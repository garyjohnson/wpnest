using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WPNest.Services {

	public interface INestWebServiceDeserializer {
		IEnumerable<Structure> ParseStructuresFromGetStatusResult(string responseString, string userId);
		Structure ParseStructureFromGetStructureStatusResult(string responseString, string structureId);
		void UpdateThermostatStatusFromSharedStatusResult(string strContent, Thermostat thermostatToUpdate);
		string ParseAccessTokenFromLoginResult(string responseString);
		DateTime ParseAccessTokenExpiryFromLoginResult(string responseString);
		string ParseUserIdFromLoginResult(string responseString);
		string ParseTransportUrlFromResult(string responseString);
		FanMode ParseFanModeFromDeviceSubscribeResult(string responseString);
		Task<WebServiceError> ParseWebServiceErrorAsync(Exception exception);
		bool ParseLeafFromDeviceSubscribeResult(string responseString);
	}

}
