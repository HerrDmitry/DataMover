using System.IO;
using FileReader;
using NUnit.Framework;

namespace DataMoverTests
{
    [TestFixture]
    public class ReaderTests
    {
        [Test]
        public void CsvReadTest()
        {
            var s = "'a','b','c'";
            var stream = GetStreamFromString(s);
            var reader = stream.CsvReader((string key) =>
            {
                switch (key)
                {
                    case "delimiter":
                        return ',';
                    case "qualifier":
                        return '\'';
                    default:
                        return null;
                }
            });
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a",row().ToString());
            Assert.AreEqual("b",row().ToString());
            Assert.AreEqual("c",row().ToString());
        }
        private MemoryStream GetStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;        
        }

    }
}