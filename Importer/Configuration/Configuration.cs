using System.Collections.Generic;
using System.Linq;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class Configuration : IConfiguration
    {
        [JsonIgnore]
        public IList<IFile> Sources { get; private set; }
        [JsonIgnore]
        public IList<IFile> Targets { get; private set; }

        [JsonIgnore]
        public IList<ILogConfiguration> LogFiles { get; private set; }

        [JsonProperty("sources")]
        public List<File> SourcesInternal
        {
            set => Sources = value?.Cast<IFile>().ToList();
        }
        [JsonProperty("targets")]
        public List<File> TargetsInternal
        {
            set => Targets = value?.Cast<IFile>().ToList();
        }

        [JsonProperty("logFiles")]
        public List<Log> LogFilesInternal
        {
            set => LogFiles = value?.Cast<ILogConfiguration>().ToList();
        }
    }
}