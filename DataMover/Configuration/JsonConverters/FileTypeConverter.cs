using System;
using Newtonsoft.Json;

namespace DataMover.Configuration.JsonConverters
{
    public class FileTypeConverter:JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
			var enumString = (string)reader.Value;

            return Enum.Parse(typeof(FileType), enumString, true);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
