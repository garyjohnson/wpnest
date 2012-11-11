using System;
using System.Threading.Tasks;
using WPNest.Login;

namespace WPNest.Tests {

	public class MockNestWebService : INestWebService {

		public bool WasLoginCalled { get; private set; }

		public async Task<LoginResult> LoginAsync(string userName, string password) {
			return await new Task<LoginResult>(() => new LoginResult());
		}

		public async Task<GetThermostatsResult> GetThermostatsAsync(string transportUrl, string accessToken, string user) {
			return await new Task<GetThermostatsResult>(() => new GetThermostatsResult(null));
		}
	}
}
