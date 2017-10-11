using System.Collections.Generic;

namespace Interfaces
{
    public interface IFileReader
    {
        IEnumerable<IDataRow> Read(IEnumerable<IRawLine> lines, IContext context);
    }
}