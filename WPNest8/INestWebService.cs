using System.Threading.Tasks;
using WPNest.Services;

namespace WPNest {
	
	public interface INestWebService {

		Task<LoginResult> LoginAsync(string userName, string password);
		Task<GetStatusResult> GetStatusAsync(string transportUrl, string accessToken, string user);
	}
}
