using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestWebServiceDeserializerTest {

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResult() {
			var deserializer = new NestWebServiceDeserializer();
			var structures = deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultAwayMode, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResultWhenFalse() {
			var deserializer = new NestWebServiceDeserializer();
			var structures = deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsFalse(structures.ElementAt(0).IsAway);
		}
	}
}
