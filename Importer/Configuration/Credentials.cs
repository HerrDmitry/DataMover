using Interfaces.Configuration;

namespace Importer.Configuration
{
	public class Credentials:ICredentials
	{
		public string Name { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string Token { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string EntryPoint { get; set; }
	}
}