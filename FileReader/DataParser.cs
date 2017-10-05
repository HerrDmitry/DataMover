using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<ISourceRow> ParseData(this Func<Func<StringBuilder>> readerFunc, Func<string, object> getValueFunc)
        {
            if (getValueFunc == null)
            {
                throw new ArgumentNullException(Localization.GetLocalizationString("getValue cannot be null"));
            }
            long sourceLineNumber = 0;
            long rowNumber = 0;
            if (!(getValueFunc("SourceConfiguration") is IFile fileConfig))
            {
                throw new ArgumentException(Localization.GetLocalizationString("Could not get Source Configuration..."));
            }
            var parsers = new List<Tuple<IRecord,List<Tuple<IColumn,Func<StringBuilder, IValue>>>>>();
            foreach (var record in fileConfig.Records)
            {
                var recordParsers = new Tuple<IRecord, List<Tuple<IColumn, Func<StringBuilder, IValue>>>>(record,
                    new List<Tuple<IColumn, Func<StringBuilder, IValue>>>());
                parsers.Add(recordParsers);
                recordParsers.Item2.AddRange(record.Columns.Select(column =>
                    new Tuple<IColumn, Func<StringBuilder, IValue>>(column,
                        GetValueParser(column.Type, column.Format))));
            }
            var currentRecord = 0;
            return () =>
            {
                int lineNumber = 0;
                SourceRow row = null;
                string error = "";
                var parsedColumns = new Dictionary<string, IValue>();
                for (var rIndex = 0; rIndex < parsers.Count; rIndex++)
                {
                    var record = readerFunc();
                    if (record == null)
                    {
                        return row;
                    }

                    lineNumber++;
                    
                    var sourceColumns = new List<StringBuilder>();
                    StringBuilder column;
                    while ((column = record()) != null)
                    {
                        sourceColumns.Add(column);
                    }

                    var rowParsers = parsers[rIndex];
                    if (sourceColumns.Count != rowParsers.Item2.Count)
                    {
                        error=string.Format(Localization.GetLocalizationString("Cannot parse line {0}, configuration does not match data source"),sourceLineNumber + lineNumber);
                        break;
                    }

                    for (var cIndex = 0; cIndex < rowParsers.Item2.Count; cIndex++)
                    {
                        var rowParser = rowParsers.Item2[cIndex];
                        var parsedValue = rowParser.Item2(sourceColumns[cIndex]);
                        parsedColumns.Add(rowParser.Item1.Name, parsedValue);
                        var valueError = parsedValue.GetError();
                        if (valueError != null)
                        {
                            error += $"{rowParser.Item1.Name}: {valueError}";
                        }
                    }
                    
                }
                
                row=new SourceRow(parsedColumns,error,currentRecord++,sourceLineNumber);

                sourceLineNumber += lineNumber;
                return row;
            };
        }

        private static Func<StringBuilder, IValue> GetValueParser(this ColumnType type, string format)
        {
            switch (type)
            {
                case ColumnType.Integer:
                    return GetIntegerParser(format);
                case ColumnType.Decimal:
                    return GetDecimalParser(format);
                case ColumnType.Date:
                    return GetDateTimeParser(format);
                case ColumnType.String:
                default:
                    return GetStringParser(format);
            }
        }

        private static Func<StringBuilder, IValue> GetStringParser(string format)
        {
            return source => new ValueWrapper<string>(source.ToString(), null);
        }

        private static Func<StringBuilder, IValue> GetDateTimeParser(string format)
        {
            return source =>
            {
                DateTime value=default(DateTime);
                var sourceStr = source.ToString();
                var hasError = false;
                if (!string.IsNullOrWhiteSpace(format))
                {
                    hasError = !DateTime.TryParseExact(sourceStr,
                        format.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
                }
                
                if (!string.IsNullOrWhiteSpace(format) || hasError)
                {
                    hasError = !DateTime.TryParse(sourceStr, out value);
                }

                var error = hasError ? Localization.GetLocalizationString("Could not parse date") : null;
                
                
                return new DateValueWrapper(value, error);
            };
        }

        private static Func<StringBuilder, IValue> GetDecimalParser(string format)
        {
            return source =>
            {
                var dec = '.';
                var neg = '-';
                if (!string.IsNullOrWhiteSpace(format))
                {
                    dec = format[0];
                }
                var normalizedSource = new StringBuilder();
                for (var i = 0; i < source.Length; i++)
                {
                    var c = source[i];
                    if (c < '0' && c > '9' && c != dec && (c != neg || neg == 0))
                    {
                        continue;
                    }
                    neg = (char) 0;
                    normalizedSource.Append(c);
                }
                string error = null;
                if (!decimal.TryParse(normalizedSource.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out decimal value))
                {
                    error = Localization.GetLocalizationString("Could not parse decimal");
                }
                return new ValueWrapper<decimal>(value, error);
            };
        }

        private static Func<StringBuilder, IValue> GetIntegerParser(string format)
        {
            return source =>
            {
                if (!string.IsNullOrWhiteSpace(format))
                {
                }
                string error = null;
                if (!long.TryParse(source.ToString(), out var value))
                {
                    error = Localization.GetLocalizationString("Could not parse integer");
                }

                return new ValueWrapper<long>(value, error);
            };
        }

        private class DateValueWrapper : ValueWrapper<DateTime>
        {
            public DateValueWrapper(DateTime value, string error) : base(value, error)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }

        private class ValueWrapper<T>:IValue<T>
        {
            public ValueWrapper(T value, string error)
            {
                this.Value = value;
                this.Error = error;
            }

            private readonly T Value;
            private readonly string Error;

            public T GetValue()
            {
                return this.Value;
            }

            public virtual string ToString(string format)
            {
                return Value?.ToString();
            }

            public override string ToString()
            {
                return this.ToString(null);
            }

            public string GetError()
            {
                return this.Error;
            }
        }
        
        private class SourceRow : ISourceRow
        {
            public SourceRow(string error, long rowNumber, long rawLineNumber):this(null,error,rowNumber,rawLineNumber)
            {
            }

            public SourceRow(IDictionary<string, IValue> columns, string error, long rowNumber,
                long rawLineNumber)
            {
                this.Columns = columns;
                this.Error = error;
                this.RowNumber = rowNumber;
                this.RawLineNumber = rawLineNumber;
            }

            public IDictionary<string, IValue> Columns { get; }
            public string Error { get; }
            public long RowNumber { get; }
            public long RawLineNumber { get; }
            public IValue this[string key] => this.Columns[key];
        }
        
    }
}