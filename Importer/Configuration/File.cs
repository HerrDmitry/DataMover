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
        [JsonProperty("hasHeaders")]
        public bool HasHeaders { get; set; }
        [JsonProperty("forceQualifier")]
        public bool ForceQualifier { get; set; }
        [JsonProperty("fixForExcel")]
        public bool FixForExcel { get; set; }
        [JsonProperty("surroundedQualifier")]
        public SurroundedQualifierType SurroundedQualifier { get; set; }
        [JsonProperty("trimStrings")]
        public bool TrimStrings { get; set; }
        [JsonProperty("hasLineDelimiters")]
        public bool HasLineDelimiters { get; set; }
        [JsonIgnore]
        public IFileMedia Media { get; set; }
        [JsonIgnore]
        public IList<IFileMedia> MultipleMedia { get; set; }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("includeSubfolders")]
        public bool IncludeSubfolders { get; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
        [JsonProperty("null")]
        public string NullValue { get; set; }
        [JsonProperty("format")]
        public FileFormat Format { get; set; }
        [JsonProperty("operation")]
        public DataOperation Operation { get; set; }
        [JsonIgnore]
        public IList<IRow> Rows { get; private set; }
        [JsonProperty("rows")]
        public List<Row> RowsInternal {
            set => this.Rows = value?.Cast<IRow>().ToList();
        }
        [JsonProperty("media")]
        public FileMedia MediaInternal
        {
            set => Media = value;
        }
        [JsonProperty("multipleMedia")]
        public List<FileMedia> MultipleMediaInternal
        {
            set => this.MultipleMedia = value?.Cast<IFileMedia>().ToList();
        }
    }
}