using System;
using System.Collections.Generic;
using System.Text;
using Interfaces;

namespace FileReader
{
    public static partial class Readers
    {
        public static IEnumerable<ISourceRow> ParseData(this Func<Func<StringBuilder>> reader, Func<string, object> getValue)
        {
            long sourceRowNumber = 0;
            long recordNumber = 0;
            Func<StringBuilder> record;
            while( (record=reader())!=null)
            {
                sourceRowNumber++;
                StringBuilder column;
                var columnNumber = 0;
                while ((column = record()) != null)
                {

                }
            }
            return null;
        }
    }
}