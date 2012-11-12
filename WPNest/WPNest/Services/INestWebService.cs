using System.Threading.Tasks;

namespace WPNest.Services {
	
	public interface INestWebService {

		Task<WebServiceResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetStatusAsync();
		Task<WebServiceResult> ChangeTemperatureAsync(Thermostat thermostat, double desiredTemperature);
		Task<GetTemperatureResult> GetTemperatureAsync(Thermostat thermostat);
	}
}
