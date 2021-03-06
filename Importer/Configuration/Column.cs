﻿using System.Collections.Generic;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;

namespace Importer.Configuration
{
    public class Column:IColumn
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("type")]
        public ColumnType Type { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("calendar")]
        public CalendarType? CalendarType { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isKey")]
        public bool GroupKey { get; set; }
        [JsonProperty("aggregateMethod")]
        public AggregateMethod AggregateMethod { get; set; }
    }
}