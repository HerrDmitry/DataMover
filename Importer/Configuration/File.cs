using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class File:IFile
    {
        [JsonProperty("delimiter")]
        public string Delimiter { get; set; }
        [JsonProperty("qualifier")]
        public string Qualifier { get; set; }
        [JsonProperty("media")]
        public MediaType MediaType { get; set; }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("includeSubfolders")]
        public bool IncludeSubfolders { get; }
        [JsonProperty("append")]
        public bool AppendToExisting { get; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("null")]
        public string NullValue { get; set; }
        [JsonProperty("format")]
        public FileFormat Format { get; set; }
        [JsonIgnore]
        public IList<IRow> Rows { get; private set; }
        [JsonProperty("rows")]
        public List<Row> RowsInternal {
            set => this.Rows = value?.Cast<IRow>().ToList();
        }
    }
}