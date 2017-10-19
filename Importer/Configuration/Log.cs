using Interfaces;
using Interfaces.Configuration;

namespace Importer.Configuration
{
	public class Log:ILogConfiguration
	{
		public string Name { get; set; }
		public LogLevel LogLevel { get; set; }
		public string Path { get; set; }
	}
}