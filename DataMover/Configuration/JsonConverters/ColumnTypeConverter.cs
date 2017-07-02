using System;
using Newtonsoft.Json;

namespace DataMover.Configuration.JsonConverters
{
    public class ColumnTypeConverter:JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
			var enumString = (string)reader.Value;

            return Enum.Parse(typeof(ColumnType), enumString, true);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
