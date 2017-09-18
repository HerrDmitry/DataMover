using System.Collections.Generic;
using System.IO;

namespace Interfaces
{
    public interface IStreamReader
    {
        IEnumerable<ISourceRow> Read(Stream stream);
    }
}
