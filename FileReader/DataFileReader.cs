using System;
using System.Collections.Generic;
using System.Text;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public static partial class Readers
    {
        public static IEnumerable<ISourceRow> ParseData(this Func<Func<StringBuilder>> reader, Func<string, object> getValue)
        {
            long sourceRowNumber = 0;
            long recordNumber = 0;
            Func<StringBuilder> record;
            IFile fileConfig = getValue("SourceConfiguration") as IFile;
            if (fileConfig == null)
            {
                throw new ArgumentException("Could not get Source Configuration...");
            }
            while( (record=reader())!=null)
            {
                sourceRowNumber++;
                StringBuilder column;
                var columnNumber = 0;
                while ((column = record()) != null)
                {
                    columnNumber++;

                }
            }
            return null;
        }
    }
}