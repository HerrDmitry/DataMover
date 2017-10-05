using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<Func<StringBuilder>> CsvReader(this Stream stream, Func<string, object> getValueFunc)
        {
            return new StreamReader(stream).CsvReader(getValueFunc);
        }

        public static Func<Func<StringBuilder>> CsvReader(this StreamReader stream, Func<string,object> getValueFunc)
        {
            var readNext=stream.BufferedRead();
            if (!(getValueFunc("SourceConfiguration") is ICsvFile fileConfig))
            {
                throw new ArgumentException(Localization.GetLocalizationString("Could not get Source Configuration..."));
            }

            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter)?',':fileConfig.Delimiter[0];
            var qualifier = string.IsNullOrWhiteSpace(fileConfig.Qualifier) ? '"' : fileConfig.Qualifier[0];

            var locker = new object();
            return () =>
            {
                lock (locker)
                {
                    var columns = new Queue<StringBuilder>();
                    Action<StringBuilder> enqueue = clmn =>
                    {
                        columns.Enqueue(clmn.Length > 0 ? clmn : null);
                    };
                    int c;
                    while ((c = readNext()) >= 0 && (c == '\n' || c == '\r')) ;
                    if (c < 0)
                    {
                        return null;
                    }
                    var isQualified = false;
                    var column=new StringBuilder();
                    while (c>=0)
                    {
                        if (c == qualifier)
                        {
                            isQualified = !isQualified;
                            if (isQualified && column.Length > 0)
                            {
                                column.Append((char)c);
                            }
                        }
                        else
                        if (c == delimiter && !isQualified)
                        {
                            enqueue(column);
                            column = new StringBuilder();
                        }
                        else
                        if ((c == '\n' || c == '\r') && !isQualified)
                        {
                            enqueue(column);
                            column = null;
                            break;
                        }
                        else
                        {
                            column.Append((char)c);
                        }
                        c = readNext();
                    }
                    if (c == -1 && column != null)
                    {
                        enqueue(column);
                    }

                    return () => columns.Count>0?columns.Dequeue():null;
                }
            };
        }
    }
}