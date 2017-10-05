using System;
using System.IO;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Writers
    {
        public static void WriteCsv(this Func<ISourceRow> nextRowFunc, Func<Stream> targetFunc, Func<string, object> getValueFunc)
        {
            if (!(getValueFunc("TargetConfiguration") is ICsvFile fileConfig))
            {
                throw new ArgumentException(Localization.GetLocalizationString("Could not get Target Configuration..."));
            }

            var nullValue = string.IsNullOrWhiteSpace(fileConfig.NullValue) ? "" : fileConfig.NullValue;
            var delimiter = string.IsNullOrWhiteSpace(fileConfig.Delimiter)?',':fileConfig.Delimiter[0];
            var qualifier = string.IsNullOrWhiteSpace(fileConfig.Qualifier) ? '"' : fileConfig.Qualifier[0];

            var targetStream = new StreamWriter(targetFunc());
            ISourceRow row;
            while ((row = nextRowFunc()) != null)
            {
                if (!string.IsNullOrWhiteSpace(row.Error))
                {
                    continue;
                }
                foreach (var r in fileConfig.Records)
                {
                    var columns = r.Columns;
                    for (var c = 0; c < columns.Count; c++)
                    {
                        var value = row[columns[c].Name];
                        targetStream.Write(qualifier);
                        targetStream.Write(value != null ? value.ToString(columns[c].Format) : nullValue);
                        targetStream.Write(qualifier);
                        if (columns.Count - 1 > c)
                        {
                            targetStream.Write(delimiter);
                        }
                    }
                    
                    targetStream.WriteLine();
                }
            }
            targetStream.Flush();
        }
    }
}