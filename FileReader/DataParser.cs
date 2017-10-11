using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Interfaces;
using Interfaces.Configuration;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<IDataRow> ParseData(this Func<IReadOnlyList<ISourceField>> getLineFunc, Func<IFile> getFileConfigFunc, Func<Interfaces.ILog> getLoggerFunc)
        {
            long sourceLineNumber = 0;
            long rowNumber = 0;
            var fileConfig = getFileConfigFunc();
            var logger = getLoggerFunc?.Invoke();
            if (fileConfig==null)
            {
                var msg = Localization.GetLocalizationString("Could not get Source Configuration...");
                logger?.Fatal(msg);
                throw new ArgumentException(msg);
            }
            logger?.Info(string.Format(Localization.GetLocalizationString("Parsing data from {0}"),fileConfig.Name));
            var parsers = new List<Tuple<IRow,List<Tuple<IColumn,Func<ISourceField, IValue>>>>>();
            foreach (var record in fileConfig.Rows)
            {
                var recordParsers = new Tuple<IRow, List<Tuple<IColumn, Func<ISourceField, IValue>>>>(record,
                    new List<Tuple<IColumn, Func<ISourceField, IValue>>>());
                parsers.Add(recordParsers);
                recordParsers.Item2.AddRange(record.Columns.Select(column =>
                    new Tuple<IColumn, Func<ISourceField, IValue>>(column,
                        GetValueParser(column.Type, column.Format))));
            }
            var currentRecord = 0;
            return () =>
            {
                int lineNumber = 0;
                DataRow row = null;
                string error = "";
                var parsedColumns = new Dictionary<string, IValue>();
                for (var rIndex = 0; rIndex < parsers.Count; rIndex++)
                {
                    var sourceColumns = getLineFunc();
                    if (sourceColumns == null)
                    {
                        if (row == null)
                        {
                            logger?.Info(string.Format(Localization.GetLocalizationString("Loaded {0} records from {1}"),lineNumber,fileConfig.Name));
                        }
                        return row;
                    }

                    lineNumber++;

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
                
                row=new DataRow(parsedColumns,error,currentRecord++,sourceLineNumber);

                sourceLineNumber += lineNumber;
                return row;
            };
        }

        private static Func<ISourceField, IValue> GetValueParser(this ColumnType type, string format)
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

        private static Func<ISourceField, IValue> GetStringParser(string format)
        {
            return source => new ValueWrapper<string>(source.Source, null, source.Source==null);
        }

        private static Func<ISourceField, IValue> GetDateTimeParser(string format)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new DateValueWrapper(DateTime.MinValue, null, true);
                }
                DateTime value = default(DateTime);
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


                return new DateValueWrapper(value, error, false);
            };
        }

        private static Func<ISourceField, IValue> GetDecimalParser(string format)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new ValueWrapper<decimal>(0,null,true);
                }
                var dec = '.';
                var neg = '-';
                if (!string.IsNullOrWhiteSpace(format))
                {
                    dec = format[0];
                }
                var normalizedSource = new StringBuilder();
                for (var i = 0; i < source.Source.Length; i++)
                {
                    var c = source.Source[i];
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
                return new ValueWrapper<decimal>(value, error, false);
            };
        }

        private static Func<ISourceField, IValue> GetIntegerParser(string format)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new ValueWrapper<long>(0, null, true);
                }
                if (!string.IsNullOrWhiteSpace(format))
                {
                }
                string error = null;
                if (!long.TryParse(source.ToString(), out var value))
                {
                    error = Localization.GetLocalizationString("Could not parse integer");
                }

                return new ValueWrapper<long>(value, error, false);
            };
        }

        private class DateValueWrapper : ValueWrapper<DateTime>
        {
            public DateValueWrapper(DateTime value, string error, bool isNull) : base(value, error, isNull)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }

        private class ValueWrapper<T>:IValue<T>
        {
            public ValueWrapper(T value, string error, bool isNull)
            {
                this.Value = value;
                this.Error = error;
                this.IsNull = isNull;

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

            public bool IsNull { get; }
        }
        
        private class DataRow : IDataRow
        {
            public DataRow(string error, long rowNumber, long rawLineNumber):this(null,error,rowNumber,rawLineNumber)
            {
            }

            public DataRow(IDictionary<string, IValue> columns, string error, long rowNumber,
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