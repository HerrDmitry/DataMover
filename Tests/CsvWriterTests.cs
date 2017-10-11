using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using FileReader;
using Importer.Configuration;
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
                    new DataRow{Columns = new Dictionary<string, IValue>{{"A", new IntegerValue(123)},{"B", new DateValue(new DateTime(2017,10,05))}}},
                    new DataRow{Columns = new Dictionary<string, IValue>{{"A", new IntegerValue(321)},{"B", new DateValue(new DateTime(2017,10,06))}}}
                };

                var index = 0;
                return () => index < rows.Count ? rows[index++] : null;
            }

            var stream=new MemoryStream();
            RowFunc().WriteCsv(()=>stream, key =>
            {
                if (key == "TargetConfiguration")
                {
                    return new Importer.Configuration.File
                    {
                        Delimiter = ",",
                        Qualifier = "\"",
                        RowsInternal = new List<Row>
                        {
                            new Row()
                            {
                                ColumnsInternal = new List<Column>
                                {
                                    new Column{Name = "A",Type = ColumnType.Integer},
                                    new Column{Name = "B",Type = ColumnType.Date,Format = "ddMMyyyy"}
                                }
                            }
                        }
                    };
                }

                return null;
            });
            
            stream.Flush();
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var myStr = sr.ReadToEnd();
            Assert.AreEqual("\"123\",\"05102017\"\n\"321\",\"06102017\"\n",myStr);
        }

        private class DataRow : IDataRow
        {
            public IDictionary<string, IValue> Columns { get; set; }
            public string Error { get; set; }
            public long RowNumber { get; set; }
            public long RawLineNumber { get; set; }

            public IValue this[string key] => Columns[key];
        }

        private class DateValue : Value<DateTime>
        {
            public DateValue(DateTime value, string error = null):base(value,error)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }
        private class IntegerValue : Value<long>
        {
            public IntegerValue(long value, string error = null):base(value,error)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }
        private class StringValue : Value<string>
        {
            public StringValue(string value, string error = null):base(value,error)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue();
            }
        }
        private abstract class Value<T>: IValue<T>
        {
            private readonly T value;
            private readonly string error;

            protected Value(T value, string error = null, bool isNull=false)
            {
                this.value = value;
                this.error = error;
                this.IsNull = isNull;
            }

            public T GetValue()
            {
                return this.value;
            }

            public abstract string ToString(string format);

            public override string ToString()
            {
                return this.ToString(null);
            }

            public string GetError()
            {
                return this.error;
            }
            
            public bool IsNull { get; }
        }
    }
}