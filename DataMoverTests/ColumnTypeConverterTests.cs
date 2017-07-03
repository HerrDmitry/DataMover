using System;
using DataMover.Configuration;
using DataMover.Configuration.JsonConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DataMoverTests
{
    [TestClass]
    public class ColumnTypeConverterTests
    {
        [TestMethod]
        public void TestColumnTypeConverter()
        {
            var converter = new ColumnTypeConverter();
            var names = Enum.GetNames(typeof(ColumnType));
            var reader = new JsonReaderTest();
            foreach(var name in names){
                reader.SetValue(name.ToLower());
                var type= (ColumnType)converter.ReadJson(reader,null,null,null);
                var expectedType = Enum.Parse(typeof(ColumnType), name, true);
                Assert.AreEqual(expectedType,type,"types should be equal");
            }
        }

        public class JsonReaderTest : JsonReader
        {
            public override bool Read()
            {
                throw new NotImplementedException();
            }

            public void SetValue(Object value){
                this.SetToken(JsonToken.Raw,value);
            }
        }
    }
}
