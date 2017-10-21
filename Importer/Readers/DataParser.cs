using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Importer.Configuration;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Readers
{
    public static partial class Readers
    {
        public static Func<IDataRow> ParseData(this Func<ISourceRow> getLineFunc, IFileConfiguration fileConfig, Interfaces.ILog logger)
        {
            if (fileConfig==null)
            {
                var msg = Localization.GetLocalizationString("Could not get Source Configuration...");
                logger?.Fatal(msg);
                throw new ArgumentException(msg);
            }
            logger?.Info(string.Format(Localization.GetLocalizationString("Parsing data from {0}"),fileConfig.Name));
            var parsers = fileConfig.GetRowParsers();
            var currentRecord = (long) 0;
            var parsedRecords = (long) 0;
            return () =>
            {
                var line = getLineFunc();
                if (line != null)
                {
                    currentRecord++;
                    try
                    {
                        var row = parsers(line, currentRecord, currentRecord);
                        if (row != null)
                        {
                            parsedRecords++;
                            return row;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(Localization.GetLocalizationString("Failed to parse line: \"{0}\"", string.Join(",", line.Fields.Select(x => x.Source))));
                        throw ex;
                    }


                    return new DataRow(
                        new Dictionary<string, IValue>
                        {
                            {
                                "raw",
                                new ValueWrapper<string>(string.Join(",", line.Fields.Select(x => x.Source)),
                                    Localization.GetLocalizationString("Parse error"), true, string.Join(",", line.Fields.Select(x => x.Source)))
                            }
                        },
                        Localization.GetLocalizationString("Could not parse line."),currentRecord, line.LineNumber, line.Context.SourcePath);
                }
                
                logger?.Info(string.Format(Localization.GetLocalizationString("Parsed {0}/{1} records from {2}"),parsedRecords,currentRecord,fileConfig.Name));
                return null;
            };
        }

        private static Func<ISourceField, IValue> GetValueParser(this IColumn column, IFileConfiguration fileConfig)
        {
            switch (column.Type)
            {
                case ColumnType.Integer:
                    return GetIntegerParser(column.Format);
                case ColumnType.Decimal:
                case ColumnType.Money:
                    return GetDecimalParser(column.Format);
                case ColumnType.Date:
                    return column.GetDateTimeParser();
                case ColumnType.String:
                default:
                    return GetStringParser(column.Format, fileConfig);
            }
        }

        private static Func<ISourceField, IValue> GetStringParser(string format, IFileConfiguration fileConfig)
        {
            if (fileConfig.TrimStrings)
            {
                return source => new ValueWrapper<string>(source.Source?.Trim(), null, source.Source==null, source.Source);
            }
            return source => new ValueWrapper<string>(source.Source, null, source.Source==null, source.Source);
        }

        private static Func<ISourceField, IValue> GetDateTimeParser(this IColumn column)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new DateValueWrapper(DateTime.MinValue, null, true, null);
                }
                DateTime value = default;
                var sourceStr = source.ToString();
                var hasError = false;
                if (!string.IsNullOrWhiteSpace(column.Format))
                {
                    hasError = !DateTime.TryParseExact(sourceStr,
                        column.Format.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
                }

                if (string.IsNullOrWhiteSpace(column.Format) || hasError)
                {
                    hasError = !DateTime.TryParse(sourceStr, out value);
                }

                if (!hasError && column.CalendarType == CalendarType.Julian)
                {
                    value = new JulianCalendar().ToDateTime(value.Year, value.Month, value.Day, value.Hour,
                        value.Minute, value.Second, value.Millisecond);
                }
                var error = hasError ? Localization.GetLocalizationString("Could not parse date \"{0}\" ",source.Source) : null;

                return new DateValueWrapper(value, error, false, source.Source);
            };
        }

        private static Func<ISourceField, IValue> GetDecimalParser(string format)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new ValueWrapper<decimal>(0,null,true, null);
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
                    error = Localization.GetLocalizationString("Could not parse decimal \"{0}\" ",source.Source);
                }
                return new ValueWrapper<decimal>(value, error, false, source.Source);
            };
        }

        private static Func<ISourceField, IValue> GetIntegerParser(string format)
        {
            return source =>
            {
                if (source?.Source == null)
                {
                    return new ValueWrapper<long>(0, null, true, null);
                }
                if (!string.IsNullOrWhiteSpace(format))
                {
                }
                string error = null;
                if (!long.TryParse(source.ToString(), out var value))
                {
                    error = Localization.GetLocalizationString("Could not parse integer \"{0}\" ",source.Source);
                }

                return new ValueWrapper<long>(value, error, false, source.Source);
            };
        }

        private static Func<ISourceRow,long,long, IDataRow> GetRowParsers(this IFileConfiguration fileConfig)
        {
            var parsers= fileConfig.Rows.Select(x => x.GetRowParser(fileConfig)).ToList();
            return (source,rowNumber,rawLineNumber) =>
            {
                return parsers.Select(parser => parser(source,rowNumber,rawLineNumber)).FirstOrDefault(result => result != null);
            };
        }

        private static Func<ISourceRow,long,long, IDataRow> GetRowParser(this IRow row, IFileConfiguration fileConfig)
        {
            var filter = row.PrepareSourceFilter();
            var parsers = row.Columns.Select(x=>x.GetValueParser(fileConfig)).ToList();

            return (source,rowNumber,rawLineNumber) =>
            {
                if (!filter(source.Fields))
                {
                    return null;
                }

                var values = new Dictionary<string, IValue>();
                var columnsCount = Math.Min(source.Fields.Count, parsers.Count);
                var error = new StringBuilder();
                for (var i = 0; i < columnsCount; i++)
                {
                    var value = parsers[i](source.Fields[i]);
                    error.Append(value.GetError() ?? "");
                    values[row.Columns[i].Name] = value;
                }
                return new DataRow(values, error.ToString(), rowNumber, rawLineNumber,source.Context.SourcePath);
            };
        }

        private class DateValueWrapper : ValueWrapper<DateTime>
        {
            public DateValueWrapper(DateTime value, string error, bool isNull, string source) : base(value, error, isNull, source)
            {
            }

            public override string ToString(string format)
            {
                return this.GetValue().ToString(format);
            }
        }

        private class ValueWrapper<T>:IValue<T>
        {
            public ValueWrapper(T value, string error, bool isNull,string source)
            {
                this.Value = value;
                this.Error = error;
                this.IsNull = isNull;
                this.Source = source;
            }

            private T Value;
            private readonly string Error;
            public string Source { get; }
            public void Update(IValue newValue)
            {
                this.Update(AggregateMethod.Last, newValue);
            }

            public void Update(AggregateMethod method, IValue value)
            {
                if (value is IValue<T> typedValue)
                {
                    switch (method)
                    {
                        case AggregateMethod.Last:
                            this.Value = typedValue.GetValue();
                            break;
                        case AggregateMethod.Join:
                            if (typeof(T)==typeof(string))
                            {
                                var stringValue = value as IValue<string>;
                                if ( this.Value==null || (this.Value as string)?.Contains(stringValue.GetValue())==false)
                                {
                                    (this as ValueWrapper<string>).Value = string.Concat((this.Value as string)??"", ",", stringValue.GetValue());
                                }
                            }
                            break;
                        case AggregateMethod.Sum:
                            if (typeof(T) == typeof(long))
                            {
                                (this as ValueWrapper<long>).Value += (value as IValue<long>)?.GetValue() ?? 0;
                            }
                            break;
                    }
                }
                
            }

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
            public DataRow(IDictionary<string, IValue> columns, string error, long rowNumber,
                long rawLineNumber, string sourcePath)
            {
                this.Columns = columns;
                this.Error = error;
                this.RowNumber = rowNumber;
                this.RawLineNumber = rawLineNumber;
                this.SourcePath = sourcePath;
            }

            public IDictionary<string, IValue> Columns { get; }
            public string Error { get; }
            public long RowNumber { get; }
            public long RawLineNumber { get; }

            public IValue this[string key] => this.Columns.TryGetValueDefault(key);
            public string SourcePath { get; }
        }

        private class RowParser
        {
            public List<IColumn> Columns { get; }
            public List<Func<ISourceField, IValue>> FieldParsers { get; }
            public List<IFilter> Filters { get; }
        }

    }
}