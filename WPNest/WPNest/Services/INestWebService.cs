using System.Threading.Tasks;

namespace WPNest.Services {
	
	public interface INestWebService {

		Task<WebServiceResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetStatusAsync();
		Task<WebServiceResult> RaiseTemperatureAsync(Thermostat thermostat);
		Task<WebServiceResult> LowerTemperatureAsync(Thermostat thermostat);
		Task<GetTemperatureResult> GetTemperatureAsync(Thermostat thermostat);
	}
}
