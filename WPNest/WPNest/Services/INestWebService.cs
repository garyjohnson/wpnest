using System.Threading.Tasks;
using WPNest.Services;

namespace WPNest {
	
	public interface INestWebService {

		Task<LoginResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetStatusAsync(string transportUrl, string accessToken, string userId);
		Task<WebServiceResult> RaiseTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat);
		Task<WebServiceResult> LowerTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat);
		Task<GetTemperatureResult> GetTemperatureAsync(string transportUrl, string accessToken, string userId, Thermostat thermostat);
	}
}
