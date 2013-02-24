using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WPNest.Services;

namespace WPNest.Test.UnitTests {

	[TestClass]
	public class NestWebServiceDeserializerTest {

		private NestWebServiceDeserializer _deserializer;

		[TestInitialize]
		public void SetUp() {
			_deserializer = new NestWebServiceDeserializer();	
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResult() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResultAwayMode, FakeJsonMessages.UserId);

			Assert.IsTrue(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureIsAwayFromGetStatusResultWhenFalse() {
			var structures = _deserializer.ParseStructuresFromGetStatusResult(FakeJsonMessages.GetStatusResult, FakeJsonMessages.UserId);

			Assert.IsFalse(structures.ElementAt(0).IsAway);
		}

		[TestMethod]
		public void ShouldParseStructureFromGetStructureStatusResult() {
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, "");

			Assert.IsFalse(structure.IsAway, "Expected Structure.IsAway to be parsed.");
		}

		[TestMethod]
		public void ShouldAssignStructureIdToGetStructureStatusResult() {
			string expectedId = "expectedId";
			Structure structure = _deserializer.ParseStructureFromGetStructureStatusResult(FakeJsonMessages.GetStructureStatusResult, expectedId);

			Assert.AreEqual(expectedId, structure.ID);
		}
	}
}
