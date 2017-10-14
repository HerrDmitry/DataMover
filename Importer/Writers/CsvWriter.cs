﻿using System;
using System.IO;
using System.Linq;
using Importer.Configuration;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Writers
{
    public static partial class Writers
    {
        public static Func<IDataRow,long> WriteCsv(this StreamWriter stream, IFile fileConfig, Interfaces.ILog log)
        {
            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter)?',':fileConfig.Delimiter[0];
            var qualifier = string.IsNullOrWhiteSpace(fileConfig.Qualifier) ? '"' : fileConfig.Qualifier[0];
            var filters = fileConfig.Rows.Select(x => x.PrepareTargetFilter()).ToList();
            var rowCount = (long) 0;
            return row =>
            {
                if (!string.IsNullOrWhiteSpace(row.Error))
                {
                    return rowCount;
                }
                for (var r = 0; r < fileConfig.Rows.Count; r++)
                {
                    if (!filters[r](row))
                    {
                        continue;
                    }
                    var columns = fileConfig.Rows[r].Columns;
                    for (var c = 0; c < columns.Count; c++)
                    {
                        var value = row[columns[c].Name];
                        var valueString = value != null ? value.ToString(columns[c].Format) : nullValue;
                        var needsQualifier = value!=null && (valueString?.Contains(qualifier)==true || fileConfig.ForceQualifier);
                        if (needsQualifier)
                        {
                            stream.Write(qualifier);
                        }
                        stream.Write(valueString);
                        if (needsQualifier)
                        {
                            stream.Write(qualifier);
                        }
                        if (columns.Count - 1 > c)
                        {
                            stream.Write(delimiter);
                        }
                    }

                    stream.WriteLine();
                    rowCount++;
                }

                return rowCount;
            };
        }
    }
}