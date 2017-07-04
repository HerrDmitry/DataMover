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
            for (var b = 2; b < s.Length; b+=2)
            {
                var loader = new CsvLoader(delimiter:'\'', readBufferSize:b);
                var rows = loader.ReadLines(GetStreamFromString(s)).ToList();
                Assert.AreEqual(3, rows.Count, $"Buffer size {b} failed");
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