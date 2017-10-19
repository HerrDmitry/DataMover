using System.IO;
using Interfaces.Configuration;

namespace Interfaces
{
	public interface ISourceFileContext
	{
		string SourcePath { get; }
		StreamReader Stream { get; }
		IFileMedia FileMedia { get; }
		IFileConfiguration FileConfiguration { get; }
	}
}