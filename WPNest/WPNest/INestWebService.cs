using System.Threading.Tasks;
using WPNest.Login;

namespace WPNest {
	
	public interface INestWebService {
		Task<LoginResult> LoginAsync(string userName, string password);
		Task<double> GetTemperatureAsync(string accessToken);
	}
}
