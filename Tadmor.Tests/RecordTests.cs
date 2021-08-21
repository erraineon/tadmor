using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Tadmor.Tests
{
    [TestClass]
    public class RecordTests
    {
        [TestMethod]
        public void Deserialize_Json_To_Record_Succeeds()
        {
            var input = @"{""foo"": ""baz"", ""bar"": 42}";
            var record = JsonConvert.DeserializeObject<TestRecord>(input);
            Assert.AreEqual("baz", record.Foo);
            Assert.AreEqual(42, record.Bar);
        }

        [TestMethod]
        public void Deserialize_Partial_Json_To_Record_Succeeds()
        {
            var input = @"{""bar"": 42}";
            var record = JsonConvert.DeserializeObject<TestRecord>(input);
            Assert.AreEqual(null, record.Foo);
            Assert.AreEqual(42, record.Bar);
        }

        private sealed record TestRecord(string Foo, int Bar);
    }
}
