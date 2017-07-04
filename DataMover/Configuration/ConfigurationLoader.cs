using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DataMover.Logger;
using Newtonsoft.Json;

namespace DataMover.Configuration
{
    public static class ConfigurationLoader
    {
        public static MigrationConfiguration ParseConfiguration(string configJson)
        {
            DataMoverLog.DebugAsync("Deserializing configuration string");
            var config = JsonConvert.DeserializeObject<MigrationConfiguration>(configJson);
            DataMoverLog.DebugAsync("Deserialized configuration string");
            return config;
        }

        public static MigrationConfiguration LoadConfiguration(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                DataMoverLog.InfoAsync($"Loading configuration from file \"{filePath}\"");
                try
                {
                    var configJson = System.IO.File.ReadAllText(filePath);
					DataMoverLog.InfoAsync($"Done reading configuration from file \"{filePath}\"");
					return ParseConfiguration(configJson);
                }
                catch (Exception ex)
                {
                    DataMoverLog.ErrorAsync(ex.Message);
                }
            }
            else
            {
                DataMoverLog.ErrorAsync($"File \"{filePath}\" does not exists");
            }

            return null;
        }

        public static MigrationConfiguration LoadConfiguration(string[] commandLineArgs)
        {
            if (commandLineArgs.Length > 0 &&
                commandLineArgs[0].StartsWith("/c:", StringComparison.CurrentCultureIgnoreCase))
            {
                var config = LoadConfiguration(commandLineArgs[0].Substring(3));
                if (config != null)
                {
                    for (var i = 1; i < commandLineArgs.Length; i++)
                    {
                        var arg = commandLineArgs[i];
                        if (arg.StartsWith("/f:"))
                        {
                            var file = arg.Substring(3).Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                            if (file.Length == 2)
                            {
                                if (config.Files == null)
                                {
                                    config.Files = new List<File>();
                                    var f = config.Files.FirstOrDefault(x =>
                                        x.Name.Equals(file[0], StringComparison.CurrentCultureIgnoreCase));
                                    if (f == null)
                                    {
                                        f = new File {Name = file[0]};
                                        config.Files.Add(f);
                                    }
                                    f.Path = file[1];
                                }
                            }
                            else
                            {
                                DataMoverLog.ErrorAsync($"Parameter \"{arg}\" is not valid.");
                            }
                        }
                        else
                        {
                            DataMoverLog.ErrorAsync($"Unknown application parameter \"{arg}\"");
                        }
                    }
                    if (ValidateConfiguration(config))
                    {
                        return config;
                    }
                }
            }
            
            return null;
        }

        private static bool ValidateConfiguration(MigrationConfiguration config)
        {
            DataMoverLog.DebugAsync("Validating configuration...");
            foreach (var source in config.Sources)
            {
                if (!config.Files.Any(x => x.Name.Equals(source.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    DataMoverLog.ErrorAsync($"Configuration source \"{source.Name}\" does not have corresponding file path.");
                    return false;
                }
            }
            
            DataMoverLog.DebugAsync("Configuration seems to be ok.");
            return true;
        }
    }
}