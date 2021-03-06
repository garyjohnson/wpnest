﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Moq;
using WPNest.Services;
using WPNest.Web;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestWebServiceTest {

		protected static string BaseUrl = "http://fakeurl.com";

		internal static NestWebService _webService;
		internal static Mock<IWebRequestProvider> _requestProvider;
		internal static Mock<IAnalyticsService> _analytics;
		internal static Mock<ISessionProvider> _sessionProvider;
		internal static Mock<IWebRequest> _webRequest;
		internal static Mock<IWebHeaderCollection> _webHeaderCollection;
		internal static Mock<INestWebServiceDeserializer> _webServiceDeserializer;
		internal static Mock<IWebResponse> _webResponse;
		internal static Mock<ITimestampProvider> _timestampProvider;

		public class NestWebServiceTestBase {

			[TestInitialize]
			public void SetUp() {
				_webServiceDeserializer = new Mock<INestWebServiceDeserializer>();
				_webRequest = new Mock<IWebRequest>();
				_webResponse = new Mock<IWebResponse>();
				_requestProvider = new Mock<IWebRequestProvider>();
				_analytics = new Mock<IAnalyticsService>();
				_sessionProvider = new Mock<ISessionProvider>();
				_webHeaderCollection = new Mock<IWebHeaderCollection>();
				_timestampProvider = new Mock<ITimestampProvider>();

				_sessionProvider.SetupGet(s => s.TransportUrl).Returns(BaseUrl);
				_requestProvider.Setup(r => r.CreateRequest(It.IsAny<Uri>())).Returns(_webRequest.Object);
				_webRequest.Setup(w => w.SetRequestStringAsync(It.IsAny<string>())).Returns(Task.Delay(0));
				_webRequest.SetupGet(w => w.Headers).Returns(_webHeaderCollection.Object);
				_webHeaderCollection.SetupSet(w => w[It.IsAny<string>()] = It.IsAny<string>());
				_webRequest.Setup(w => w.GetResponseAsync()).Returns(Task.FromResult(_webResponse.Object));
				_webResponse.Setup(w => w.GetResponseStream()).Returns(new MemoryStream());
				_webResponse.Setup(w => w.GetResponseStringAsync()).Returns(Task.FromResult(""));
				_webServiceDeserializer.Setup(d => d.ParseStructureFromGetStructureStatusResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new Structure(""));
				_webServiceDeserializer.Setup(d => d.ParseWebServiceErrorAsync(It.IsAny<Exception>())).Returns(Task.FromResult(WebServiceError.Unknown));
				_timestampProvider.Setup(t => t.GetTimestamp()).Returns(1234567890d);

				ServiceContainer.RegisterService<INestWebServiceDeserializer>(_webServiceDeserializer.Object);
				ServiceContainer.RegisterService<IWebRequestProvider>(_requestProvider.Object);
				ServiceContainer.RegisterService<IAnalyticsService>(_analytics.Object);
				ServiceContainer.RegisterService<ISessionProvider>(_sessionProvider.Object);
				ServiceContainer.RegisterService<ITimestampProvider>(_timestampProvider.Object);

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
		public class NestWebService_WhenCallingGetStructureStatus : NestWebServiceTestBase {

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
			public async Task ShouldSendResponseStringToDeserializer() {
				_webResponse.Setup(w => w.GetResponseStringAsync()).Returns(Task.FromResult(FakeJsonMessages.GetStructureStatusResult));

				await _webService.GetStructureAndDeviceStatusAsync(new Structure("structureId"));

				_webServiceDeserializer.Verify(d => d.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, "structureId"));
			}

			[TestMethod]
			public async Task ShouldReturnStructureFromDeserializer() {
				var expectedStructure = new Structure("structureId");
				_webServiceDeserializer.Setup(d => d.ParseStructureFromGetStructureStatusResult(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedStructure);

				GetStatusResult result = await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				Assert.AreEqual(expectedStructure, result.Structures.ElementAt(0));
			}

			[TestMethod]
			public async Task ShouldReturnExceptionWhenGetResponseFails() {
				var expectedException = new Exception("Failed!");
				_webRequest.Setup(r => r.GetResponseAsync()).Throws(expectedException);

				GetStatusResult result = await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				Assert.AreEqual(expectedException, result.Exception);
			}

			[TestMethod]
			public async Task ShouldReturnErrorWhenGetResponseFails() {
				_webRequest.Setup(r => r.GetResponseAsync()).Throws(new Exception());
				_webServiceDeserializer.Setup(d => d.ParseWebServiceErrorAsync(It.IsAny<Exception>())).Returns(Task.FromResult(WebServiceError.SessionTokenExpired));

				GetStatusResult result = await _webService.GetStructureAndDeviceStatusAsync(new Structure(""));

				Assert.AreEqual(WebServiceError.SessionTokenExpired, result.Error);
			}

			[TestMethod]
			public async Task ShouldAddDeviceKeyForThermostats() {
				string thermostatId = "12345";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat(thermostatId));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey = string.Format("\"key\":\"device.{0}\"", thermostatId);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey))));
			}

			[TestMethod]
			public async Task ShouldGetFanModeFromDeserializer() {
				_webResponse.SetupSequence(w => w.GetResponseStringAsync())
				            .Returns(Task.FromResult(FakeJsonMessages.GetStructureStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetSharedStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetDeviceStatusResult));

				var structure = new Structure("");
				structure.Thermostats.Add(new Thermostat(""));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				_webServiceDeserializer.Verify(d => d.ParseFanModeFromDeviceSubscribeResult(FakeJsonMessages.GetDeviceStatusResult));
			}

			[TestMethod]
			public async Task ShouldGetLeafFromDeserializer() {
				_webResponse.SetupSequence(w => w.GetResponseStringAsync())
				            .Returns(Task.FromResult(FakeJsonMessages.GetStructureStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetSharedStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetDeviceStatusResult));

				var structure = new Structure("");
				structure.Thermostats.Add(new Thermostat(""));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				_webServiceDeserializer.Verify(d => d.ParseLeafFromDeviceSubscribeResult(FakeJsonMessages.GetDeviceStatusResult));
			}

			[TestMethod]
			public async Task ShouldAddThermostatsToResult() {
				var expectedFanMode = FanMode.On;
				_webServiceDeserializer.Setup(d => d.ParseFanModeFromDeviceSubscribeResult(It.IsAny<string>()))
				                       .Returns(expectedFanMode);
				_webResponse.SetupSequence(w => w.GetResponseStringAsync())
				            .Returns(Task.FromResult(FakeJsonMessages.GetStructureStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetDeviceStatusResult));

				var structure = new Structure("");
				structure.Thermostats.Add(new Thermostat(""));
				structure.Thermostats.Add(new Thermostat(""));

				GetStatusResult result = await _webService.GetStructureAndDeviceStatusAsync(structure);

				Assert.AreEqual(2, result.Structures.ElementAt(0).Thermostats.Count);
			}

			[TestMethod]
			public async Task ShouldUseFanModeFromDeserializer() {
				var expectedFanMode = FanMode.On;
				_webServiceDeserializer.Setup(d => d.ParseFanModeFromDeviceSubscribeResult(It.IsAny<string>()))
				                       .Returns(expectedFanMode);
				_webResponse.SetupSequence(w => w.GetResponseStringAsync())
				            .Returns(Task.FromResult(FakeJsonMessages.GetStructureStatusResult))
				            .Returns(Task.FromResult(FakeJsonMessages.GetDeviceStatusResult));

				var structure = new Structure("");
				structure.Thermostats.Add(new Thermostat(""));
				GetStatusResult result = await _webService.GetStructureAndDeviceStatusAsync(structure);
				Assert.AreEqual(expectedFanMode, result.Structures.ElementAt(0).Thermostats[0].FanMode);
			}

			[TestMethod]
			public async Task ShouldNotAddDeviceKeyForThermostatsWhenGetStructureStatusFails() {
				_webRequest.Setup(r => r.GetResponseAsync()).Throws(new Exception());
				_webServiceDeserializer.Setup(d => d.ParseWebServiceErrorAsync(It.IsAny<Exception>())).Returns(Task.FromResult(WebServiceError.SessionTokenExpired));

				string thermostatId = "12345";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat(thermostatId));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey = string.Format("\"key\":\"device.{0}\"", thermostatId);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey))), Times.Never());
			}

			[TestMethod]
			public async Task ShouldStopGettingDeviceStatusIfOneFails() {
				_webRequest.SetupSequence(r => r.GetResponseAsync())
				           .Returns(Task.FromResult(_webResponse.Object))
				           .Throws(new Exception());
				_webServiceDeserializer.Setup(d => d.ParseWebServiceErrorAsync(It.IsAny<Exception>())).Returns(Task.FromResult(WebServiceError.SessionTokenExpired));

				string secondThermostatId = "12345";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat("id"));
				structure.Thermostats.Add(new Thermostat(secondThermostatId));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey = string.Format("\"key\":\"device.{0}\"", secondThermostatId);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey))), Times.Never());
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
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey1))), Times.Once());
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey2))), Times.Once());
			}

			[TestMethod]
			public async Task ShouldStopGettingDeviceStatusIfSharedStatusFails() {
				_webRequest.SetupSequence(r => r.GetResponseAsync())
				           .Returns(Task.FromResult(_webResponse.Object))
				           .Throws(new Exception());
				_webServiceDeserializer.Setup(d => d.ParseWebServiceErrorAsync(It.IsAny<Exception>())).Returns(Task.FromResult(WebServiceError.SessionTokenExpired));

				string firstThermostatId = "12345";
				var structure = new Structure("id");
				structure.Thermostats.Add(new Thermostat(firstThermostatId));
				structure.Thermostats.Add(new Thermostat("id"));
				await _webService.GetStructureAndDeviceStatusAsync(structure);

				string expectedKey = string.Format("\"key\":\"device.{0}\"", firstThermostatId);
				_webRequest.Verify(w => w.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedKey))), Times.Never());
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

		[TestClass]
		public class NestWebService_WhenSettingAwayMode : NestWebServiceTestBase {
			
			[TestMethod]
			public async Task ShouldUseCorrectUrl() {
				var structure = new Structure("id123");
				await _webService.SetAwayMode(structure, true);

				var expectedUri = new Uri(BaseUrl + "/v2/put/structure.id123");
				_requestProvider.Verify(r => r.CreateRequest(expectedUri));
			}

			[TestMethod]
			public async Task ShouldSetAwayModeInRequestString() {
				await _webService.SetAwayMode(new Structure(""), true);

				string expectedString = "\"away\":true";
				_webRequest.Verify(r=>r.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedString))));
			}

			[TestMethod]
			public async Task ShouldSetAwaySetterInRequestString() {
				await _webService.SetAwayMode(new Structure(""), true);

				string expectedString = "\"away_setter\":0";
				_webRequest.Verify(r=>r.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedString))));
			}

			[TestMethod]
			public async Task ShouldSetAwayTimestampInRequestString() {
				double timestamp = 1234567890d;
				_timestampProvider.Setup(t => t.GetTimestamp()).Returns(timestamp);

				await _webService.SetAwayMode(new Structure(""), true);

				string expectedString = "\"away_timestamp\":" + timestamp;
				_webRequest.Verify(r=>r.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedString))));
			}

			[TestMethod]
			public async Task ShouldSetAuthorizationHeaderOnRequest() {
				string accessToken = "token";
				_sessionProvider.SetupGet(s => s.AccessToken).Returns(accessToken);
				await _webService.SetAwayMode(new Structure(""), true);

				_webHeaderCollection.VerifySet(w => w["Authorization"] = "Basic " + accessToken);
			}

			[TestMethod]
			public async Task ShouldSetMethodToPost() {
				await _webService.SetAwayMode(new Structure(""), true);

				_webRequest.VerifySet(w => w.Method = "POST");
			}

			[TestMethod]
			public async Task ShouldSetContentTypeToJson() {
				await _webService.SetAwayMode(new Structure(""), true);

				_webRequest.VerifySet(w => w.ContentType = ContentType.Json);
			}

			[TestMethod]
			public async Task ShouldSetNestHeadersOnRequest() {
				string userId = "userId";
				_sessionProvider.SetupGet(s => s.UserId).Returns(userId);
				await _webService.SetAwayMode(new Structure(""), true);

				_webHeaderCollection.VerifySet(w => w["X-nl-protocol-version"] = "1");
				_webHeaderCollection.VerifySet(w => w["X-nl-user-id"] = userId);
			}
		}

		[TestClass]
		public class NestWebService_WhenSettingHvacMode : NestWebServiceTestBase {
			
			[TestMethod]
			public async Task ShouldUseCorrectUrl() {
				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				var expectedUri = new Uri(BaseUrl + "/v2/put/shared.id123");
				_requestProvider.Verify(r => r.CreateRequest(expectedUri));
			}

			[TestMethod]
			public async Task ShouldSetHvacModeInRequestString() {
				_webServiceDeserializer.Setup(d => d.GetHvacModeString(It.IsAny<HvacMode>())).Returns("testing");

				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				string expectedString = "\"target_temperature_type\":\"testing\"";
				_webRequest.Verify(r=>r.SetRequestStringAsync(It.Is<string>(s => s.Contains(expectedString))));
			}

			[TestMethod]
			public async Task ShouldSetAuthorizationHeaderOnRequest() {
				string accessToken = "token";
				_sessionProvider.SetupGet(s => s.AccessToken).Returns(accessToken);
				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				_webHeaderCollection.VerifySet(w => w["Authorization"] = "Basic " + accessToken);
			}

			[TestMethod]
			public async Task ShouldSetMethodToPost() {
				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				_webRequest.VerifySet(w => w.Method = "POST");
			}

			[TestMethod]
			public async Task ShouldSetContentTypeToJson() {
				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				_webRequest.VerifySet(w => w.ContentType = ContentType.Json);
			}

			[TestMethod]
			public async Task ShouldSetNestHeadersOnRequest() {
				string userId = "userId";
				_sessionProvider.SetupGet(s => s.UserId).Returns(userId);
				var thermostat = new Thermostat("id123");
				await _webService.SetHvacModeAsync(thermostat, HvacMode.Off);

				_webHeaderCollection.VerifySet(w => w["X-nl-protocol-version"] = "1");
				_webHeaderCollection.VerifySet(w => w["X-nl-user-id"] = userId);
			}
		}
	}
}
