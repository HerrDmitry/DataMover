namespace Interfaces.Configuration
{
	public interface ILogConfiguration
	{
		string Name { get; }
		LogLevel LogLevel { get; }
		string Path { get; }
	}
}