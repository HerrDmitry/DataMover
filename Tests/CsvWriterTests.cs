using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using FileReader;
using Importer;
using Importer.Configuration;
using Importer.Writers;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class CsvWriterTests:TestBase
    {
        [TestMethod]
        public void WriteTest()
        {
            Func<IDataRow> RowFunc()
            {
                var rows = new List<IDataRow>
                {
                    new DataRow{Columns = new Dictionary<string, IValue>{{"A", new IntegerValue(123)},{"B", new DateValue(new DateTime(2017,10,05))},{"C",new StringValue("a,b,\"c\"")}}},
                    new DataRow{Columns = new Dictionary<string, IValue>{{"A", new IntegerValue(321)},{"B", new DateValue(new DateTime(2017,10,06))},{"C", new StringValue("a\"b")}}}
                };

                var index = 0;
                return () => index < rows.Count ? rows[index++] : null;
            }

            var stream=new MemoryStream();
            var streamWriter=new StreamWriter(stream);
            var csvWriter = streamWriter.GetCsvWriter(new Importer.Configuration.File
            {
                Delimiter = ",",
                Qualifier = "\"",
                ForceQualifier = false,
                RowsInternal = new List<Row>
                {
                    new Row()
                    {
                        ColumnsInternal = new List<Column>
                        {
                            new Column {Name = "A", Type = ColumnType.Integer},
                            new Column {Name = "B", Type = ColumnType.Date, Format = "ddMMyyyy"},
                            new Column {Name = "C", Type = ColumnType.String}
                        }
                    }
                }
            }, new ConsoleLogger());

            var nextRow = RowFunc();
            while (true)
            {
                var row = nextRow();
                if (row == null)
                {
                    break;
                }
                csvWriter(row);
            }

            streamWriter.Flush();
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var myStr = sr.ReadToEnd();
            Assert.AreEqual("123,05102017,\"a,b,\\\"c\\\"\""+Environment.NewLine+"321,06102017,\"a\\\"b\""+Environment.NewLine,myStr);
        }

    }
}