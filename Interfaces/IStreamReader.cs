using System.Collections.Generic;
using System.IO;

namespace Interfaces
{
    public interface IStreamReader
    {
        IEnumerable<IRawLine> Read(Stream stream, IContext context);
    }
}
