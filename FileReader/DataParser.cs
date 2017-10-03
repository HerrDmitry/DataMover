using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<ISourceRow> ParseData(this Func<Func<StringBuilder>> reader, Func<string, object> getValue)
        {
            long sourceLineNumber = 0;
            long rowNumber = 0;
            var fileConfig = getValue("SourceConfiguration") as IFile;
            if (fileConfig == null)
            {
                throw new ArgumentException("Could not get Source Configuration...");
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
                var columns = new Dictionary<string, IValue>();
                int lineNumber = 0;
                SourceRow row = null;
                string error = "";
                var parsedColumns = new Dictionary<string, IValue>();
                for (var rIndex = 0; rIndex < parsers.Count; rIndex++)
                {
                    var record = reader();
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
                        error=$"Cannot parse line {sourceLineNumber + lineNumber}, configuration does not match data source";
                        break;
                    }

                    for (var cIndex = 0; cIndex < rowParsers.Item2.Count; cIndex++)
                    {
                        var rowParser = rowParsers.Item2[cIndex];
                        parsedColumns.Add(rowParser.Item1.Name, rowParser.Item2(sourceColumns[cIndex]));
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
                    return GetIntegerParse(format);
                case ColumnType.String:
                default:
                    return GetStringParser(format);
            }
        }

        private static Func<StringBuilder, IValue> GetStringParser(string format)
        {
            return source => new ValueWrapper<string>(source.ToString(), null);
        }

        private static Func<StringBuilder, IValue> GetIntegerParse(string format)
        {
            return source =>
            {
                if (!string.IsNullOrWhiteSpace(format))
                {
                }
                var error = "";
                if (int.TryParse(source.ToString(), out var value))
                {
                    error = "Could not parse integer";
                }

                return new ValueWrapper<int>(value, error);
            };
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

            public string ToString(string format)
            {
                return Value?.ToString();
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
        }
        
    }
}