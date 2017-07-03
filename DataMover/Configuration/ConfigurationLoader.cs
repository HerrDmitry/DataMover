using System.IO;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public static class ConfigurationLoader
    {
        public static MigrationConfiguration ParseConfiguration(string configJson)
        {
            return JsonConvert.DeserializeObject<MigrationConfiguration>(configJson);
        }

        public static MigrationConfiguration LoadConfiguration(string filePath)
        {
            if (File.Exists(filePath))
            {
                var configJson = File.ReadAllText(filePath);
                return ParseConfiguration(configJson);
            }

            return null;
        }
    }
}