using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;

namespace FileReader
{
    public class CsvReader:IStreamReader
    {
        public IEnumerable<IRawLine> Read(Stream stream, IContext config)
        {
            throw new NotImplementedException();
        }
    }
}