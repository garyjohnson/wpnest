using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.IntegrationTests {

	[TestClass]
	public class NestWebServiceIntegrationTest {

		private NestWebService _webService;

		[TestInitialize]
		public void SetUp() {
			ServiceContainer.Clear();
			ServiceContainer.RegisterService<ISettingsProvider>(new SettingsProvider());
			ServiceContainer.RegisterService<IWebRequestProvider>(new WebRequestProvider());
			ServiceContainer.RegisterService<IAnalyticsService>(new AnalyticsService());
			ServiceContainer.RegisterService<ISessionProvider>(new SessionProvider());

			_webService = new NestWebService();
		}

		[TestCleanup]
		public void TearDown() {
			ServiceContainer.Clear();
			_webService = null;
		}

		[TestMethod]
		[Ignore]
		public async Task ShouldGetStructureStatus() {
			await _webService.LoginAsync("gary@gjtt.com", "YesMaam");
			var t = await _webService.GetFullStatusAsync();
			await _webService.GetStructureStatusAsync(t.Structures.ElementAt(0));
		}
	}
}
