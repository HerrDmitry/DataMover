using System.Collections.Generic;

namespace Interfaces.Configuration
{
    public interface IRow
    {
        IList<IColumn> Columns { get; }
        IList<IFilter> Filter { get; }
        
    }
}