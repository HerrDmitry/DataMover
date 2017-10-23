using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public abstract class TestBase
    {
        private Stopwatch watch;

        [TestInitialize]
        public void Initialize()
        {
            watch=new Stopwatch();
            watch.Start();
        }

        [TestCleanup]
        public void Cleanup()
        {
            watch.Stop();
            Console.WriteLine($"Test finished in {watch.ElapsedMilliseconds}ms");
        }

        protected MemoryStream GetStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
                protected class DataRow : IDataRow
        {
            public IDictionary<string, IValue> Columns { get; set; }
            public string Error { get; set; }
            public long RowNumber { get; set; }
            public long RawLineNumber { get; set; }

            public IValue this[string key] => Columns[key];
            public string SourcePath { get; set; }
        }

        protected class DateValue : Value<DateTime>
        {
            public DateValue(DateTime value, string error=null,bool isNull=false ):base(value,error,isNull,value.ToString())
            {
            }

            public override Type GetValueType()
            {
                return typeof(DateTime);
            }

            public override IValue Assign(IValue value)
            {
                throw new NotImplementedException();
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }
        protected class IntegerValue : Value<long>
        {
            public IntegerValue(long value, string error=null, bool isNull=false):base(value,error,isNull,value.ToString())
            {
            }

            public override Type GetValueType()
            {
                throw new NotImplementedException();
            }

            public override IValue Assign(IValue value)
            {
                throw new NotImplementedException();
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }
        protected class StringValue : Value<string>
        {
            public StringValue(string value, string error=null, bool isNull=false):base(value,error,isNull,value)
            {
            }

            public override Type GetValueType()
            {
                throw new NotImplementedException();
            }

            public override IValue Assign(IValue value)
            {
                throw new NotImplementedException();
            }

            public override string ToString(string format)
            {
                return this.GetValue();
            }
        }
        protected abstract class Value<T>: IValue<T>
        {
            private readonly T value;
            private readonly string error;

            protected Value(T value, string error, bool isNull, string source)
            {
                this.value = value;
                this.error = error;
                this.IsNull = isNull;
                this.Source = source;
            }

            public T GetValue()
            {
                return this.value;
            }

            public string Source { get; }
            public void Update(IValue newValue)
            {
                throw new NotImplementedException();
            }

            public void Update(AggregateMethod method, IValue value)
            {
                throw new NotImplementedException();
            }

            public abstract Type GetValueType();
            public abstract IValue Assign(IValue value);

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