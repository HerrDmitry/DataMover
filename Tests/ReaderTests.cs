using System.IO;
using System.Threading;
using FileReader;
using Importer;
using Importer.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ReaderTests :TestBase
    {
        [TestMethod]
        public void CsvReadTest1()
        {
            var s = "'a','b','c'";
            var stream = GetStreamFromString(s);
            var reader = new StreamReader(stream).BufferedRead(new ConsoleLogger()).CsvReader( new Importer.Configuration.File
            {
                Delimiter = ",",
                Qualifier = "'",
                Name="Test"
            },new ConsoleLogger());
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a", row[0].ToString());
            Assert.AreEqual("b", row[1].ToString());
            Assert.AreEqual("c", row[2].ToString());
            Assert.IsNull(reader());
        }
        [TestMethod]
        public void CsvReadTest2()
        {
            var s = "'a''b','b','c'\n'd','e\ne',f";
            var reader = new StreamReader(GetStreamFromString(s)).BufferedRead(new ConsoleLogger()).CsvReader(
                new Importer.Configuration.File
                {
                    Delimiter = ",",
                    Qualifier = "'"
                }, new ConsoleLogger());
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a'b", row[0].ToString());
            Assert.AreEqual("b", row[1].ToString());
            Assert.AreEqual("c", row[2].ToString());
            row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("d", row[0].ToString());
            Assert.AreEqual("e\ne", row[1].ToString());
            Assert.AreEqual("f", row[2].ToString());
            row = reader();
            Assert.IsNull(row);
        }

        [TestMethod]
        public void CsvReadTest3()
        {
            var s = "'a''b',,'c'\n'd','e\ne',f";
            var reader = new StreamReader(GetStreamFromString(s)).BufferedRead(new ConsoleLogger()).CsvReader(
                new Importer.Configuration.File
                {
                    Delimiter = ",",
                    Qualifier = "'"
                }, new ConsoleLogger());
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a'b", row[0].ToString());
            Assert.IsTrue(row[1].Source==null);
            Assert.AreEqual("c", row[2].ToString());
        }
    }
}
