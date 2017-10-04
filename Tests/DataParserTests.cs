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
    public class DataParserTests:TestBase
    {

        [TestMethod]
        public void DataParserTestEmptyReader()
        {
            var s = "'a',1,'c'";
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
            var parser = reader.ParseData(key =>
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
                                    new FileConfiguration.FileColumnConfiguration {Name = "A", Type = ColumnType.String},
                                    new FileConfiguration.FileColumnConfiguration {Name = "B", Type = ColumnType.Integer},
                                    new FileConfiguration.FileColumnConfiguration {Name = "C", Type = ColumnType.String}
                                }
                            }
                        }

                    };
                }
                return null;
            });
            var row = parser();
            Assert.IsNotNull(row);
            Console.WriteLine(row.Error);
            Assert.IsTrue(string.IsNullOrEmpty(row.Error));
            Assert.AreEqual(row.Columns.Count,3);
            Assert.AreEqual(row["A"].ToString(),"a");
            Assert.AreEqual(row["B"].ToString(),"1");
            Assert.AreEqual(row["C"].ToString(),"c");
            Assert.IsInstanceOfType(row["B"], typeof(IValue<int>));
            Assert.AreEqual(((IValue<int>)row["B"]).GetValue(),1);
        }
    }
}