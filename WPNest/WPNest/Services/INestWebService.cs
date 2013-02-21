using System.Threading.Tasks;

namespace WPNest.Services {
	
	public interface INestWebService {

		Task<WebServiceResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetFullStatusAsync();
		Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature);
		Task<GetThermostatStatusResult> GetThermostatStatusAsync(Thermostat thermostat);
		Task<WebServiceResult> SetFanModeAsync(Thermostat thermostat, FanMode fanMode);
		Task<WebServiceResult> UpdateTransportUrlAsync();
	}
}
