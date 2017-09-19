using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;

namespace CsvReader
{
    public class Reader:IStreamReader
    {
        public IEnumerable<IRawLine> Read(Stream stream, IConfiguration config)
        {
            throw new NotImplementedException();
        }
    }
}