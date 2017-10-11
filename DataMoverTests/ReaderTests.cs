using System.IO;
using FileReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;

namespace DataMoverTests
{
    [TestClass]
    public class ReaderTests: TestBase
    {
        [TestMethod]
        public void CsvReadTest()
        {
            var s = "'a','b','c'";
            var stream = GetStreamFromString(s);
            var reader = stream.CsvReader(() => new Importer.Configuration.File
            {
                Delimiter = ",",
                Qualifier = "'"
            },()=>new TestBase.ConsoleLogger());
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a",row[0].ToString());
            Assert.AreEqual("b",row[1].ToString());
            Assert.AreEqual("c",row[2].ToString());
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