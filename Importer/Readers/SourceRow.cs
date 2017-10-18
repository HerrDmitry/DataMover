using System.Collections.Generic;
using Interfaces;

namespace Importer.Readers
{
	public class SourceRow:ISourceRow
	{
		public IReadOnlyList<ISourceField> Fields { get; set; }
		public ISourceFileContext Context { get; set; }
	}
}