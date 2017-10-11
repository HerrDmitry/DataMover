using System.Collections.Generic;
using System.Linq;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class Row:IRow
    {
        [JsonProperty("columns")]
        public List<Column> ColumnsInternal {
            set => this.Columns = value?.Cast<IColumn>().ToList();
        }
        
        [JsonProperty("filter")]
        public List<Filter> FilterInternal
        {
            set => this.Filter = value?.Cast<IFilter>().ToList();
        }

        [JsonIgnore]
        public IList<IColumn> Columns { get; private set; }
        
        [JsonIgnore]
        public IList<IFilter> Filter { get; private set; }
    }
}