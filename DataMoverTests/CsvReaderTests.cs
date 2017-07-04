using System.IO;
using System.Linq;
using System.Text;
using DataMover.Loaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMoverTests
{
    [TestClass]
    public class CsvReaderTests
    {
        [TestMethod]
        public void ReadOneLineFile()
        {
            var s = "'a','b','c'";
            var loader = new CsvLoader('\'');
            var rows = loader.ReadLines(GetStreamFromString(s)).ToList();
            Assert.AreEqual(1,rows.Count);
            Assert.AreEqual(3,rows[0].Columns.Length);
            Assert.AreEqual("a",rows[0].Columns[0]);
            Assert.AreEqual("b",rows[0].Columns[1]);
            Assert.AreEqual("c",rows[0].Columns[2]);
        }

        [TestMethod]
        public void ReadMultipleLinesWithChangingBufferSize()
        {
            var s = "'a','b','c'\r\n'd','e','f'\r'g','h','i'\nj,k,l";
            for (var b = 1; b < s.Length; b+=1)
            {
                var loader = new CsvLoader(textQualifier:'\'', readBufferSize:b);
                var rows = loader.ReadLines(GetStreamFromString(s)).ToList();
                Assert.AreEqual(4, rows.Count, $"Buffer size {b} failed");
                Assert.AreEqual("a",rows[0].Columns[0]);
                Assert.AreEqual("b",rows[0].Columns[1]);
                Assert.AreEqual("c",rows[0].Columns[2]);
                Assert.AreEqual("d",rows[1].Columns[0]);
                Assert.AreEqual("e",rows[1].Columns[1]);
                Assert.AreEqual("f",rows[1].Columns[2]);
                Assert.AreEqual("g",rows[2].Columns[0]);
                Assert.AreEqual("h",rows[2].Columns[1]);
                Assert.AreEqual("i",rows[2].Columns[2]);
                Assert.AreEqual("j",rows[3].Columns[0]);
                Assert.AreEqual("k",rows[3].Columns[1]);
                Assert.AreEqual("l",rows[3].Columns[2]);
            }
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