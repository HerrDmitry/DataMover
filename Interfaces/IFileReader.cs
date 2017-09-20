using System.Collections.Generic;

namespace Interfaces
{
    public interface IFileReader
    {
        IEnumerable<ISourceRow> Read(IEnumerable<IRawLine> lines, IContext context);
    }
}