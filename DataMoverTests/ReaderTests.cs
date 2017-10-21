using System.IO;
using Importer;
using Importer.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;
using File = Importer.Configuration.File;

namespace DataMoverTests
{
    [TestClass]
    public class ReaderTests : TestBase
    {
        [TestMethod]
        public void CsvReadTest()
        {
            var s = "'a','b','c'";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "'"
            };
            var log = new ConsoleLogger();
            var reader = context.BufferedRead(log).CsvReader(context,  log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a", row.Fields[0].ToString());
            Assert.AreEqual("b", row.Fields[1].ToString());
            Assert.AreEqual("c", row.Fields[2].ToString());
        }
    }
}