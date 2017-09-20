using System.Collections.Generic;

namespace Interfaces.FileDefinition
{
    public interface IFile
    {
        string Name { get; }
        IList<IRecord> Records { get; }
    }
}