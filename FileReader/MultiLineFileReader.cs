using System.Collections.Generic;
using Interfaces;

namespace FileReader
{
    public class MultiLineFileReader:IFileReader
    {
        public IEnumerable<ISourceRow> Read(IEnumerable<IRawLine> lines, IContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}