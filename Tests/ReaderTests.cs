using System.IO;
using Importer;
using Importer.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File = Importer.Configuration.File;

namespace Tests
{
    [TestClass]
    public class ReaderTests :TestBase
    {
        [TestMethod]
        public void CsvReadTest1()
        {
            var s = "'a','b','c'";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "'",
                Name="Test"
            };
            var log = new ConsoleLogger();

            var reader = context.BufferedRead(log).CsvReader(context, log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a", row.Fields[0].ToString());
            Assert.AreEqual("b", row.Fields[1].ToString());
            Assert.AreEqual("c", row.Fields[2].ToString());
            Assert.IsNull(reader());
        }
        [TestMethod]
        public void CsvReadTest2()
        {
            var s = "'a''b','b','c'\n'd','e\ne',f";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "'",
                Name="Test"
            };
            var log = new ConsoleLogger();

            var reader = context.BufferedRead(log).CsvReader(context, log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a'b", row.Fields[0].ToString());
            Assert.AreEqual("b", row.Fields[1].ToString());
            Assert.AreEqual("c", row.Fields[2].ToString());
            row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("d", row.Fields[0].ToString());
            Assert.AreEqual("e\ne", row.Fields[1].ToString());
            Assert.AreEqual("f", row.Fields[2].ToString());
            row = reader();
            Assert.IsNull(row);
        }

        [TestMethod]
        public void CsvReadTest3()
        {
            var s = "'a''b',,'c'\n'd','e\ne',f";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "'",
                Name="Test"
            };
            var log = new ConsoleLogger();

            var reader = context.BufferedRead(log).CsvReader(context, log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a'b", row.Fields[0].ToString());
            Assert.IsTrue(row.Fields[1].Source==null);
            Assert.AreEqual("c", row.Fields[2].ToString());
        }
        
        [TestMethod]
        public void CsvReadTest4()
        {
            var s = "\"a'b\",,\"c\"\n\"d\",\"e\ne\",f";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "\"",
                Name="Test"
            };
            var log = new ConsoleLogger();

            var reader = context.BufferedRead(log).CsvReader(context, log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a'b", row.Fields[0].ToString());
            Assert.IsTrue(row.Fields[1].Source==null);
            Assert.AreEqual("c", row.Fields[2].ToString());
        }
        
        [TestMethod]
        public void CsvReadTest5()
        {
            var s = "a\"b,,\"c\"\n\"d\",\"e\ne\",f";
            var context = new SourceFileContext();
            var stream = GetStreamFromString(s);
            context.Stream = new StreamReader(stream);
            context.FileConfiguration = new File
            {
                Delimiter = ",",
                Qualifier = "\"",
                Name="Test"
            };
            var log = new ConsoleLogger();

            var reader = context.BufferedRead(log).CsvReader(context, log);
            Assert.IsNotNull(reader);
            var row = reader();
            Assert.IsNotNull(row);
            Assert.AreEqual("a\"b", row.Fields[0].ToString());
            Assert.IsTrue(row.Fields[1].Source==null);
            Assert.AreEqual("c", row.Fields[2].ToString());
        }
    }
}
