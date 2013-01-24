using System.Threading.Tasks;

namespace WPNest.Services {
	
	public interface INestWebService {

		Task<WebServiceResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetStatusAsync();
		Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature);
		Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat);
//		Task<WebServiceResult> SetAwayModeAsync(Structure structure, bool isAway);
		Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode);
//		Task<WebServiceResult> SetHvacModeAsync(Thermostat thermostat, HvacMode hvacMode);
	}
}
