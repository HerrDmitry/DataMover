using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Tests
{
  [TestClass]
  public class ConfigTests:TestBase
  {
    [TestMethod]
    public void ConfigurationParseTest()
    {
      var config = JsonConvert.DeserializeObject<Importer.Configuration.Configuration>(sampleConfig);
      Assert.IsNotNull(config);
      Assert.AreEqual(ColumnType.String,config.Sources[0].Rows[0].Columns[0].Type);
      Assert.AreEqual(ColumnType.Integer,config.Sources[0].Rows[0].Columns[1].Type);
      Assert.AreEqual(ColumnType.Decimal,config.Sources[0].Rows[0].Columns[2].Type);
      Assert.AreEqual("HDR",config.Targets[0].Rows[0].Filter[0].Value);
      Assert.AreEqual("selector",config.Targets[0].Rows[0].Filter[0].Name);
    }

    private const string sampleConfig = @"
{
  'sources': [
    {
      'name': 'transactions',
      'path': 'transactions.txt',
      'media': 'ftp',
      'format': 'csv',
      'delimiter': ',',
      'qualifier': '\'',
      'login': 'login',
      'password': 'password',
      'rows': [
        {
          'columns': [
            {'name': 'selector','type': 'string'},
            {'name': 'id1','type': 'integer'},
            {'name': 'id2','type': 'decimal'},
            {'name': 'id3','type': 'string'}
          ],
          'filter':[{'name':'selector','value':'HDR'}]
        }
      ]
    }
  ],
  'targets':[
    {
      'name':'headers',
      'path':'headers.txt',
      'format':'csv',
      'media':'local',
      'delimiter':',',
      'rows':[{'columns':[{'name':'id1'},{'name':'id2'},{'name':'id3'}],
      'filter':[{'name':'selector','value':'HDR'}]
}]
    },
    {
      'name':'details',
      'rows':[{'columns':[{'name':'id1'},{'name':'id2'},{'name':'id3'}]}],
      'filter':[{'name':'selector','value':'DTL'}]
    },
    {
      'name':'taxes',
      'rows':[{'columns':[{'name':'id1'},{'name':'id2'},{'name':'id3'}]}],
      'filter':[{'name':'selector','value':'TAX'}]
    },
    {
      'name':'media',
      'rows':[{'columns':[{'name':'id1'},{'name':'id2'},{'name':'id3'}]}],
      'filter':[{'name':'selector','value':'MED'}]
    }
  ]
}
";
  }
}