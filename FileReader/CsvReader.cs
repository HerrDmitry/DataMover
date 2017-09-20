using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;

namespace FileReader
{
    public class CsvReader:IStreamReader
    {
        public IEnumerable<IRawLine> Read(Func<Stream> getStream, Func<object,string> getValue)
        {
            throw new NotImplementedException();
        }
    }
}