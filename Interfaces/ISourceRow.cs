using System.Collections.Generic;

namespace Interfaces
{
	public interface ISourceRow
	{
		IReadOnlyList<ISourceField> Fields { get; }
		ISourceFileContext Context { get; }
	}
}