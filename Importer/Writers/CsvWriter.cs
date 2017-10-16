using System;
using System.IO;
using System.Linq;
using System.Text;
using Importer.Configuration;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Writers
{
    public static partial class Writers
    {
        private const string DEFAULT_QUALIFIER = "\"";
        private const string DEFAULT_DELIMITER = ",";
        public static Func<IDataRow,long> WriteCsv(this StreamWriter stream, IFile fileConfig, Interfaces.ILog log)
        {
            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter)?DEFAULT_DELIMITER[0]:fileConfig.Delimiter[0];
            var qualifier = string.IsNullOrWhiteSpace(fileConfig.Qualifier) ? DEFAULT_QUALIFIER[0] : fileConfig.Qualifier[0];
            var filters = fileConfig.Rows.Select(x => x.PrepareTargetFilter()).ToList();
            var rowCount = (long) 0;
            var getQualifiedString = fileConfig.GetQualifiedStrFunc();
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
                        var valueString = value != null ? getQualifiedString(value.ToString(columns[c].Format)) : nullValue;
                        stream.Write(valueString);
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
        private static Func<string, string> GetQualifiedStrFunc(this ICsvFile fileConfig)
        {
            var qualifier = fileConfig.Qualifier?.Length > 0 ? fileConfig.Qualifier[0].ToString() : DEFAULT_QUALIFIER;
            var delimiter = fileConfig.Delimiter?.Length > 0 ? fileConfig.Delimiter[0].ToString() : DEFAULT_DELIMITER;
            var replaceQualifier = qualifier.getReplaceQualifierFunc(fileConfig.SurroundedQualifier);
            
            if (fileConfig.ForceQualifier)
            {
                return source => string.Concat(qualifier, replaceQualifier(source)??"", qualifier);
            }

            return source =>
            {
                if (source.IndexOf(qualifier) > -1 || source.IndexOf(delimiter) > -1)
                {
                    return string.Concat(qualifier, replaceQualifier(source)??"", qualifier);
                }
                return source;
            };
        }

        private static Func<string, string> getReplaceQualifierFunc(this string qualifier, SurroundedQualifierType? qt)
        {
            var qualifierReplacement =
                (qt == SurroundedQualifierType.Double ? qualifier : "\\") + qualifier;
            return source => string.IsNullOrWhiteSpace(source)?source:new StringBuilder(source).Replace(qualifier, qualifierReplacement).ToString();
        }
    }
}