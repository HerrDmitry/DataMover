using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Readers
    {
        public static IEnumerable<ISourceRow> ParseData(this Func<Func<StringBuilder>> reader, Func<string, object> getValue)
        {
            long sourceRowNumber = 0;
            long recordNumber = 0;
            Func<StringBuilder> record;
            IFile fileConfig = getValue("SourceConfiguration") as IFile;
            if (fileConfig == null)
            {
                throw new ArgumentException("Could not get Source Configuration...");
            }
            while( (record=reader())!=null)
            {
                sourceRowNumber++;
                StringBuilder column;
                var columnNumber = 0;
                while ((column = record()) != null)
                {
                    columnNumber++;

                }
            }
            return null;
        }

        public static Func<StringBuilder, IValue> GetValueParser(this ColumnType type, string format)
        {
            switch (type)
            {
                case ColumnType.String:
                default:
                    return value => new StringValue(value,format);
            }
        }

        private class IntegerValue : Value<int>
        {
            public IntegerValue(StringBuilder source, string format) : base(source, format, conversion
                )
            {
                
            }
            
            private static int conversion (StringBuilder s, string f, Action<string> setError)
            {
                if (int.TryParse(s.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var result))
                {
                    return result;
                }

                setError($"Failed to convert {s} to int");

                return 0;
            }
        }

        private class StringValue : Value<string>
        {
            public StringValue(StringBuilder source, string format):base(source,format, (s, f, setError) => s.ToString())
            {
            }
        }

        private class Value<T>:IValue<T>
        {
            protected readonly StringBuilder source;
            protected readonly string format;
            protected string error;
            private Func<Func<T>> conversion;

            protected Value(StringBuilder source, string format, Func<StringBuilder,string,Action<string>,T> conversionFunc)
            {
                this.source = source;
                this.format = format;
                this.error = null;
                conversion = () =>
                {
                    var val = conversionFunc(source,format, (error) => { this.error = error; });
                    return ()=>val;
                };

            }

            public T GetValue()
            {
                return conversion()();
            }

            public override string ToString()
            {
                return this.ToString(null);
            }

            public virtual string ToString(string format)
            {
                return this.ToString();
            }

            public virtual string GetError()
            {
                return this.error;
            }
        }
    }
}