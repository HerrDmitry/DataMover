﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Readers
{
    public static partial class Readers
    {
        public static Func<ISourceRow> CsvReader(this Func<int> readNext, ISourceFileContext context, Interfaces.ILog logger)
        {
            var fileConfig = context.FileConfiguration;
            if (fileConfig==null)
            {
                var msg = Localization.GetLocalizationString("Could not get Source Configuration...");
                logger?.Fatal(msg);
                throw new ArgumentException(msg);
            }

            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter) ? ',' : fileConfig.Delimiter[0];
            var qualifier = string.IsNullOrWhiteSpace(fileConfig.Qualifier) ? '\"' : fileConfig.Qualifier[0];

            var locker = new object();
            var rowCount = 0;
            logger?.Info(string.Format(Localization.GetLocalizationString("Loading data from {0}..."), fileConfig.Name));
            return () =>
            {
                lock (locker)
                {
                    var columns = new List<ISourceField>();
                    var hadQualifier = false;
                    Action<StringBuilder> enqueue = clmn =>
                    {
                        var value = clmn.ToString();
                        columns.Add((value.Length > 0 && (value!=nullValue || hadQualifier)) ? new SourceField(value) : new SourceField(null));
                    };
                    int c;
                    while ((c = readNext()) >= 0 && (c == '\n' || c == '\r')) ;
                    if (c < 0)
                    {
                        logger?.Info(Localization.GetLocalizationString("Loaded {0} line(s) from {1}.", rowCount, fileConfig.Name));
                        return null;
                    }
                    var isQualified = false;
                    var column=new StringBuilder();
                    while (c>=0)
                    {
                        if (c == qualifier && (column.Length==0 || hadQualifier))
                        {
                            hadQualifier = true;
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

                    rowCount++;
                    return new SourceRow {Context = context, Fields = columns, LineNumber=rowCount};
                }
            };
        }

        private class SourceField : ISourceField
        {
            public SourceField(string field)
            {
                Source = field;
            }

            public string Source { get; }

            public override string ToString()
            {
                return Source;
            }
        }
    }
}