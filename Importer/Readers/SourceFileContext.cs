using System.IO;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Readers
{
	public class SourceFileContext:ISourceFileContext
	{
		public string SourcePath { get; set; }
		public StreamReader Stream { get; set; }
		public IFileMedia FileMedia { get; set; }
		public IFileConfiguration FileConfiguration { get; set; }
	}
}