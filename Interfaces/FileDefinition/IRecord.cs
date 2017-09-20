using System.Collections.Generic;

namespace Interfaces.FileDefinition
{
    public interface IRecord
    {
        IList<IColumn> Columns { get; }
    }
}