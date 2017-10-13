using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FileReader;
using Importer;
using Importer.Configuration;
using Importer.Readers;
using Interfaces;
using Interfaces.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DataParserTests : TestBase
    {

        [TestMethod]
        public void DataParserTest()
        {
            var s = "'a',1,'c',2,'2017-10-01 14:15:16'\n'a1',1.1,c1,3,'2017-10-04";
            var stream = GetStreamFromString(s);
            var reader = stream.CsvReader(() => new File 
            {
                Delimiter = ",",
                Qualifier = "'"
            }, () => new ConsoleLogger());
            var parser = reader.ParseData(() => getConfig(), () => new ConsoleLogger());
            var row = parser();
            Assert.IsNotNull(row);
            Assert.IsTrue(string.IsNullOrEmpty(row.Error));
            Assert.AreEqual(5, row.Columns.Count);
            Assert.AreEqual("a", row["A"].ToString());
            Assert.AreEqual("1", row["B"].ToString());
            Assert.AreEqual("c", row["C"].ToString());
            Assert.AreEqual("2", row["D"].ToString());
            Assert.AreEqual("01/10/2017", row["E"].ToString("dd/MM/yyyy"));
            Assert.IsInstanceOfType(row["B"], typeof(IValue<decimal>));
            Assert.AreEqual(1, ((IValue<decimal>) row["B"]).GetValue());
            row = parser();
            Assert.IsTrue(string.IsNullOrEmpty(row.Error));
            Assert.AreEqual(5, row.Columns.Count);
            Assert.AreEqual("1.1", row["B"].ToString());
            Assert.IsInstanceOfType(row["B"], typeof(IValue<decimal>));
            Assert.AreEqual((decimal) 1.1, ((IValue<decimal>) row["B"]).GetValue());
        }

        [TestMethod]
        public void DataParserTestNullValue()
        {
            var s = "'a',1,,2,'2017-10-01 14:15:16'\n'a1',1.1,c1,3,'2017-10-04";
            var stream = GetStreamFromString(s);
            var reader = stream.CsvReader(() => new File
            {
                Delimiter = ",",
                Qualifier = "'"
            }, () => new ConsoleLogger());
            var parser = reader.ParseData(()=>getConfig(), () => new ConsoleLogger());
            var row = parser();
            Console.WriteLine(row.Error);
            Assert.IsTrue(string.IsNullOrEmpty(row.Error));
            Assert.AreEqual(5, row.Columns.Count);
            Assert.IsTrue(row["C"].IsNull);
        }

        private IFile getConfig()
        {
            return new File
            {
                Name = "File",
                RowsInternal = new List<Row>
                {
                    new Row
                    {
                        ColumnsInternal = new List<Column>
                        {
                            new Column
                            {
                                Name = "A",
                                Type = ColumnType.String
                            },
                            new Column
                            {
                                Name = "B",
                                Type = ColumnType.Decimal
                            },
                            new Column
                            {
                                Name = "C",
                                Type = ColumnType.String
                            },
                            new Column
                            {
                                Name = "D",
                                Type = ColumnType.Integer
                            },
                            new Column
                            {
                                Name = "E",
                                Type = ColumnType.Date,
                                Format = "yyyy-MM-dd hh:mm:ss"
                            }
                        }
                    }
                }

            };
        }
    }
}