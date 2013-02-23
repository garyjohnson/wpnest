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
		protected static Mock<IWebHeaderCollection> _webHeaderCollection;

		public class NestWebServiceTestBase {

			[TestInitialize]
			public void SetUp() {
				_webRequest = new Mock<IWebRequest>();
				_requestProvider = new Mock<IWebRequestProvider>();
				_analytics = new Mock<IAnalyticsService>();
				_sessionProvider = new Mock<ISessionProvider>();
				_webHeaderCollection = new Mock<IWebHeaderCollection>();

				_sessionProvider.SetupGet(s => s.TransportUrl).Returns(BaseUrl);
				_requestProvider.Setup(r => r.CreateRequest(It.IsAny<Uri>())).Returns(_webRequest.Object);
				_webRequest.Setup(w => w.SetRequestStringAsync(It.IsAny<string>())).Returns(Task.Delay(0));
				_webRequest.SetupGet(w => w.Headers).Returns(_webHeaderCollection.Object);
				_webHeaderCollection.SetupSet(w => w[It.IsAny<string>()] = It.IsAny<string>());

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
		public class WhenCallingGetStructureAndDeviceStatus : NestWebServiceTestBase {

			[TestMethod]
			public async Task ShouldUseCorrectUrl() {
				var structure = new Structure("id");
				await _webService.GetStructureAndDeviceStatusAsync(structure);
				var expectedUri = new Uri(BaseUrl + "/v2/subscribe");

				_requestProvider.Verify(r => r.CreateRequest(expectedUri));
			}

			[TestMethod]
			public async Task ShouldAddKeyForStructure() {
				var structureId = "id";
				var structure = new Structure(structureId);
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains("\"key\":\"structure.id\""))));
			}

			[TestMethod]
			public async Task ShouldAddDeviceKeysForThermostats() {
				string thermostatId1 = "12345";
				string thermostatId2 = "54321";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat(thermostatId1));
				structure.Thermostats.Add(new Thermostat(thermostatId2));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey1 = string.Format("\"key\":\"device.{0}\"", thermostatId1);
				string expectedKey2 = string.Format("\"key\":\"device.{0}\"", thermostatId2);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey1) && s.Contains(expectedKey2))));
			}

			[TestMethod]
			public async Task ShouldAddSharedKeysForThermostats() {
				string thermostatId1 = "12345";
				string thermostatId2 = "54321";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat(thermostatId1));
				structure.Thermostats.Add(new Thermostat(thermostatId2));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey1 = string.Format("\"key\":\"shared.{0}\"", thermostatId1);
				string expectedKey2 = string.Format("\"key\":\"shared.{0}\"", thermostatId2);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey1) && s.Contains(expectedKey2))));
			}

			[TestMethod]
			public async Task ShouldSetAuthorizationHeaderOnRequest() {
				string accessToken = "token";
				_sessionProvider.SetupGet(s => s.AccessToken).Returns(accessToken);
				await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				_webHeaderCollection.VerifySet(w => w["Authorization"] = "Basic " + accessToken);
			}

			[TestMethod]
			public async Task ShouldSetMethodToPost() {
				await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				_webRequest.VerifySet(w => w.Method = "POST");
			}

			[TestMethod]
			public async Task ShouldSetContentTypeToJson() {
				await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				_webRequest.VerifySet(w => w.ContentType = ContentType.Json);
			}

			[TestMethod]
			public async Task ShouldSetNestHeadersOnRequest() {
				string userId = "userId";
				_sessionProvider.SetupGet(s => s.UserId).Returns(userId);
				await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				_webHeaderCollection.VerifySet(w => w["X-nl-protocol-version"] = "1");
				_webHeaderCollection.VerifySet(w => w["X-nl-user-id"] = userId);
			}
		}
	}
}
