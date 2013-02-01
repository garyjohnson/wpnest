using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;
using WPNest.Test.Mocks;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestViewModelTest {

		[TestMethod]
		public async Task ShouldBlah() {
			var webService = new MockNestWebService();
			var result = await webService.GetStatusAsync();
			Assert.IsNotNull(result);
		}
	}
}
