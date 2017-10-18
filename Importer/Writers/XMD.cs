using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Importer.Writers
{
	public static partial class Writers
	{
		private class XMD
		{
			[JsonProperty("fileFormat")]
			public XMDFileFormat FileFormat { get; set; }
			
			[JsonProperty("objects")]
			public IEnumerable<XMDObject> Objects { get; set; }

			public class XMDFileFormat
			{
				[JsonProperty("charsetName")]
				public string CharsetName { get; set; }

				[JsonProperty("fieldsEnclosedBy")]
				public string FieldsEnclosedBy { get; set; }

				[JsonProperty("fieldsDelimitedBy")]
				public string FieldsDelimitedBy { get; set; }

				[JsonProperty("numberOfLinesToIgnore")]
				public int NumberOfLinesToIgnore { get; set; }
			}

			public class XMDObject
			{
				[JsonProperty("connector")]
				public string Connector => "ThinkLPCSVConnector";

				[JsonProperty("description")]
				public string Description { get; set; }

				[JsonProperty("fullyQualifiedName")]
				public string FullyQualifiedName { get; set; }

				[JsonProperty("label")]
				public string Label { get; set; }

				[JsonProperty("name")]
				public string Name { get; set; }
				
				[JsonProperty("fields")]
				public IEnumerable<XMDField> Fields { get; set; }
			}

			public class XMDField
			{
				public XMDField()
				{
					Type = XMDFieldType.Text;
				}

				[JsonProperty("description")]
				public string Description { get; set; }

				[JsonProperty("fullyQualifiedName")]
				public string FullyQualifiedName { get; set; }

				[JsonProperty("label")]
				public string Label { get; set; }

				[JsonProperty("name")]
				public string Name { get; set; }

				[JsonProperty("isSystemField")]
				public bool? IsSystemField { get; set; }

				[JsonProperty("defaultValue")]
				public string DefaultValue { get; set; }

				[JsonProperty("isUniqueId")]
				public bool? IsUniqueId { get; set; }
				
				[JsonProperty("isMultiValue")]
				public bool? IsMultiValue { get; set; }
				
				[JsonProperty("multiValueSeparator")]
				public string MultiValueSeparator { get; set; }

				[JsonProperty("type")]
				[JsonConverter(typeof(StringEnumConverter))]
				public XMDFieldType Type { get; set; }

				[JsonProperty("precision")]
				public int? Precision { get; set; }

				[JsonProperty("scale")]
				public int? Scale { get; set; }

				[JsonProperty("format")]
				public string Format { get; set; }

				[JsonProperty("fiscalMonthOffset")]
				public int? FiscalMonthOffset { get; set; }
			}

			public enum XMDFieldType
			{
				Text,
				Numeric,
				Date
			}
		}
	}
}