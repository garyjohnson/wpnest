using Microsoft.Phone.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPNest.Login;
using System.Threading.Tasks;

namespace WPNest.Tests {

	[TestClass]
	public class LoginViewModelTest : PresentationTest {

		private LoginViewModel viewModel;
		private MockNestWebService nestWebService;

		[TestMethod]
		[Ignore]
		public async Task LoginShouldCallLoginOnNestWebService() {
			viewModel = new LoginViewModel();
			await viewModel.LoginAsync();
			nestWebService = (MockNestWebService)ServiceContainer.GetService<INestWebService>();
			Assert.IsTrue(nestWebService.WasLoginCalled);
		}
	}
}
