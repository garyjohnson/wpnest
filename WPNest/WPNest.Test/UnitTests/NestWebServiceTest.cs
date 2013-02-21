using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;
using WPNest.Web;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestWebServiceTest {

		protected static string BaseUrl = "http://fakeurl.com"; 

		protected static NestWebService _webService;
		protected static Mock<IWebRequestProvider> _requestProvider;
		protected static Mock<IAnalyticsService> _analytics;
		protected static Mock<ISessionProvider> _sessionProvider;
		protected static Mock<IWebRequest> _webRequest;

		public class NestWebServiceTestBase {
			
			[TestInitialize]
			public void SetUp() {
				_webRequest = new Mock<IWebRequest>();
				_requestProvider = new Mock<IWebRequestProvider>();
				_analytics = new Mock<IAnalyticsService>();
				_sessionProvider = new Mock<ISessionProvider>();

				_sessionProvider.SetupGet(s => s.TransportUrl).Returns(BaseUrl);
				_requestProvider.Setup(r => r.CreateRequest(It.IsAny<Uri>())).Returns(_webRequest.Object);

				ServiceContainer.RegisterService<IWebRequestProvider>(_requestProvider.Object);
				ServiceContainer.RegisterService<IAnalyticsService>(_analytics.Object);
				ServiceContainer.RegisterService<ISessionProvider>(_sessionProvider.Object);

				_webService = new NestWebService();
			}

			[TestCleanup]
			public void TearDown() {
				_webService = null;
				_requestProvider = null;
				_analytics = null;
				_sessionProvider = null;
			}
		}

		[TestClass]
		public class WhenCallingGetStructureAndDeviceStatus  : NestWebServiceTestBase{

			[TestMethod]
			public void ShouldUseCorrectUrl() {
				var structure = new Structure("id");
				_webService.GetStructureAndDeviceStatusAsync(structure);
				var expectedUri = new Uri(BaseUrl + "/v2/subscribe");

				_requestProvider.Verify(r => r.CreateRequest(expectedUri));
			}
		}
	}
}
