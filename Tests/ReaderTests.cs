using System.IO;
using FileReader;
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
            var reader = stream.CsvReader(key =>
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
            Assert.AreEqual("a", row().ToString());
            Assert.AreEqual("b", row().ToString());
            Assert.AreEqual("c", row().ToString());
        }
        [TestMethod]
        public void CsvReadTest2()
        {
            var s = "'a''b','b','c'\n'd','e\ne',f";
            var reader = GetStreamFromString(s).CsvReader(key =>
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
            Assert.AreEqual("a'b", row().ToString());
            Assert.AreEqual("b", row().ToString());
            Assert.AreEqual("c", row().ToString());
            row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("d", row().ToString());
            Assert.AreEqual("e\ne", row().ToString());
            Assert.AreEqual("f", row().ToString());
            row = reader();
            Assert.IsNull(row);
        }
    }
}
