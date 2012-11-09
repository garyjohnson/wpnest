using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPNest.Login;

namespace WPNest {

	[TestClass]
	public class LoginViewModelTest : SilverlightTest {

		private LoginViewModel viewModel;

		[TestInitialize]
		public void Initialize() {
			viewModel = new LoginViewModel();
		}

		[TestMethod]
		public void ShouldLogInUsingNestWebServiceOnLogin() {

			viewModel.Login();
		}
	}
}
