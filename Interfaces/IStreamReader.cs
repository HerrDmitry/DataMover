using System;
using System.Collections.Generic;
using System.IO;

namespace Interfaces
{
    public interface IStreamReader
    {
        IEnumerable<IRawLine> Read(Func<Stream> getStream, Func<object,string> getParam);
    }
}
