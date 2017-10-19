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
        public static Func<IDataRow,long> GetCsvWriter(this StreamWriter stream, IFile fileConfig, Interfaces.ILog log)
        {
            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter)?DEFAULT_DELIMITER[0]:fileConfig.Delimiter[0];
            var filters = fileConfig.Rows.Select(x => x.PrepareTargetFilter()).ToList();
            var rowCount = (long) 0;
            var getQualifiedString = fileConfig.GetQualifiedStrFunc(log);
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
                    var rowString=new StringBuilder();
                    for (var c = 0; c < columns.Count; c++)
                    {
                        var column = columns[c];
                        var value = row[column.Name];
                        if (value == null)
                        {
                            //log.Warning(Localization.GetLocalizationString("Could not find column {0}", columns[c].Name));
                        }
                        
                        var valueString = value != null ? getQualifiedString(value.ToString(column.Format),column.Type) : nullValue;
                        rowString.Append(valueString);
                        if (columns.Count - 1 > c)
                        {
                            rowString.Append(delimiter);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(row.Error))
                    {
                        log.Error(Localization.GetLocalizationString("Error in file {0}, line {1}",row.SourcePath, row.RawLineNumber) + " - " +
                                  (row.Error ?? "") + "\n\r\t" +
                                  string.Join(",", row.Columns.Values.Select(x => x.Source)));
                    }
                    else
                    {
                        stream.WriteLine(rowString.ToString());
                    }
                    rowCount++;
                }

                return rowCount;
            };

        }
        private static Func<string, ColumnType, string> GetQualifiedStrFunc(this ICsvFile fileConfig,ILog log)
        {
            var qualifier = fileConfig.Qualifier?.Length > 0 ? fileConfig.Qualifier[0].ToString() : DEFAULT_QUALIFIER;
            var delimiter = fileConfig.Delimiter?.Length > 0 ? fileConfig.Delimiter[0].ToString() : DEFAULT_DELIMITER;
            var replaceQualifier = qualifier.getReplaceQualifierFunc(fileConfig.SurroundedQualifier, log);
            
            if (fileConfig.ForceQualifier)
            {
                return (source,type) =>
                {
                    if (type == ColumnType.String && source?.All(x => x == '0')==true)
                    {
                        source = "";
                    }
                    return string.Concat(qualifier, replaceQualifier(source) ?? "", qualifier);
                };
            }

            return (source,type) =>
            {
                if (type == ColumnType.String && source?.All(x => x == '0')==true)
                {
                    source = "";
                }
                if (source.IndexOf(qualifier) > -1 || source.IndexOf(delimiter) > -1)
                {
                    return string.Concat(qualifier, replaceQualifier(source)??"", qualifier);
                }
                return source;
            };
        }

        private static Func<string, string> getReplaceQualifierFunc(this string qualifier, SurroundedQualifierType? qt, ILog log)
        {
            var qualifierReplacement =
                (qt == SurroundedQualifierType.Double ? qualifier : "\\") + qualifier;
            log.Debug($"Replacing {qualifier} with {qualifierReplacement}");
            return source => string.IsNullOrWhiteSpace(source)?source:new StringBuilder(source).Replace(qualifier, qualifierReplacement).ToString();
        }
    }
}