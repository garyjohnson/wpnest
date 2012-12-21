using Microsoft.Phone.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WPNest.Services;
using System.Threading.Tasks;

namespace WPNest.Test {

	[TestClass]
	public class NestViewModelTest : WorkItemTest {

		private NestViewModel _viewModel;
		private Mock<INestWebService> _nestWebService;
		private MockSessionProvider _sessionProvider;
		private MockStatusProvider _statusProvider;

		[TestInitialize]
		public void SetUp() {
			_nestWebService = new Mock<INestWebService>();
			_sessionProvider = new MockSessionProvider();
			_statusProvider = new MockStatusProvider();
			ServiceContainer.RegisterService<ISessionProvider>(_sessionProvider);
			ServiceContainer.RegisterService<INestWebService>(_nestWebService.Object);
			ServiceContainer.RegisterService<IStatusProvider>(_statusProvider);
			_viewModel = new NestViewModel();
		}

		[TestMethod]
		public void ShouldSubscribeToThermostatStatusUpdatesWhenCreated() {
			var expectedResult = new GetThermostatStatusResult(12.3d, 32.1d, true, true);
			_statusProvider.FireThermostatStatusUpdated(expectedResult);

			Assert.AreEqual(expectedResult.TargetTemperature, _viewModel.TargetTemperature);
			Assert.AreEqual(expectedResult.CurrentTemperature, _viewModel.CurrentTemperature);
			Assert.AreEqual(expectedResult.IsHeating, _viewModel.IsHeating);
			Assert.AreEqual(expectedResult.IsCooling, _viewModel.IsCooling);
		}

		[TestMethod]
		[Asynchronous]
		public async Task ShouldLoginWhenSessionExpiredOnInitialize() {
			_sessionProvider.IsSessionExpired = true;

			await _viewModel.InitializeAsync();

			Assert.IsTrue(_viewModel.IsLoggingIn);
			EnqueueTestComplete();
		}

		[TestMethod]
		[Asynchronous]
		public async Task ShouldLoginWithCredentialsOnLoginIfSessionExpired() {

			_viewModel.UserName = "expected username";
			_viewModel.Password = "expected password";
			_sessionProvider.IsSessionExpired = true;
			_nestWebService.Setup(webService => webService.LoginAsync(_viewModel.UserName, _viewModel.Password)).Returns(new Task<WebServiceResult>(() => new WebServiceResult()));

			await _viewModel.LoginAsync();

			_nestWebService.Verify(webService => webService.LoginAsync(_viewModel.UserName, _viewModel.Password));
			EnqueueTestComplete();
		}
	}
}
