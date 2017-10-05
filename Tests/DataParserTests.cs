using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FileReader;
using Interfaces;
using Interfaces.FileDefinition;
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
            var reader = stream.CsvReader(key =>
            {
                switch (key)
                {
                    case "SourceConfiguration":
                        return new CsvFileConfiguration {Delimiter = ",", Qualifier = "\'"};
                    default:
                        return null;
                }
            });
            var parser = reader.ParseData(getConfig);
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
            var reader = stream.CsvReader(key =>
            {
                switch (key)
                {
                    case "SourceConfiguration":
                        return new CsvFileConfiguration {Delimiter = ",", Qualifier = "\'"};
                    default:
                        return null;
                }
            });
            var parser = reader.ParseData(getConfig);
            var row = parser();
            Console.WriteLine(row.Error);
            Assert.IsTrue(string.IsNullOrEmpty(row.Error));
            Assert.AreEqual(5, row.Columns.Count);
            Assert.IsNull(row["C"]);
        }

        private object getConfig(string key)
        {
            {
                if (key == "SourceConfiguration")
                {
                    return new FileConfiguration
                    {
                        Name = "File",
                        Records = new List<IRecord>
                        {
                            new FileConfiguration.FileRecordConfiguration
                            {
                                Columns = new List<IColumn>
                                {
                                    new FileConfiguration.FileColumnConfiguration
                                    {
                                        Name = "A",
                                        Type = ColumnType.String
                                    },
                                    new FileConfiguration.FileColumnConfiguration
                                    {
                                        Name = "B",
                                        Type = ColumnType.Decimal
                                    },
                                    new FileConfiguration.FileColumnConfiguration
                                    {
                                        Name = "C",
                                        Type = ColumnType.String
                                    },
                                    new FileConfiguration.FileColumnConfiguration
                                    {
                                        Name = "D",
                                        Type = ColumnType.Integer
                                    },
                                    new FileConfiguration.FileColumnConfiguration
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
                return null;
            }
        }
    }
}