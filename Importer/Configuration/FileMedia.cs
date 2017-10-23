using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
	public class FileMedia :IFileMedia
	{
		[JsonProperty("media")]
		public MediaType MediaType { get; set; }
		[JsonProperty("path")]
		public string Path { get; set; }
		[JsonProperty("includeSubfolders")]
		public bool IncludeSubfolders { get; set; }
		[JsonProperty("operation")]
		public DataOperation Operation { get; set; }
		[JsonProperty("disabled")]
		public bool Disabled { get; set; }
		[JsonProperty("credentials")]
		public string Credentials { get; set; }
		[JsonIgnore]
		public ICredentials ConnectionCredentials { get; set; }

		[JsonProperty("encoding")]
		public FileEncoding Encoding { get; set; }
	}
}