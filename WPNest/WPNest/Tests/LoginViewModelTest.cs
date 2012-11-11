using System.Collections.Generic;
using Microsoft.Phone.Testing;
using Microsoft.Phone.Testing.Harness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPNest.Login;
using System.Threading.Tasks;

namespace WPNest.Tests {

	[TestClass]
	public class LoginViewModelTest : Microsoft.Phone.Testing.PresentationTest {

		private LoginViewModel viewModel;
		private MockNestWebService nestWebService;

		[TestMethod]
		[Asynchronous]
		public void LoginShouldCallLoginOnNestWebService() {
			viewModel = new LoginViewModel();
			var loginTask = viewModel.LoginAsync();
			EnqueueConditional(() => loginTask.IsCompleted || loginTask.IsFaulted);
			EnqueueCallback(() => {
				EnqueueDelay(100);
				nestWebService = (MockNestWebService)ServiceContainer.GetService<INestWebService>();
				Assert.IsTrue(nestWebService.WasLoginCalled);
				EnqueueTestComplete();
			});


		}
	}
}
