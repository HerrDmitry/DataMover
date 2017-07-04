using System.Linq;
using DataMover.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataMoverTests
{
    [TestClass]
    public class ConfigParserTests
    {
        [TestMethod]
        public void ParseTest()
        {
            var configSource = @"{
                'sources':[
                    {
                        'name':'test',
                        'records':[
                        {
                            'rows':[
                            {
                                'columns':[
                                {
                                    'name':'test column 1',
                                    'type':'string'
                                },
                                {
                                    'name':'test column 2',
                                    'type':'date'
                                }]
                            },
                            {
                                'columns':[
                                {
                                    'name':'test column 3',
                                    'type':'integer'
                                },
                                {
                                    'name':'test column 4',
                                    'type':'double'
                                }]
                            }]
                        }]
                    }
                ]
            }";

            var config = ConfigurationLoader.ParseConfiguration(configSource);
            Assert.IsNotNull(config);
            Assert.AreEqual(1,config.Sources.Count);
            Assert.AreEqual(1,config.Sources[0].Records.Count());
            Assert.AreEqual(2,config.Sources[0].Records.First().Rows.Count);
            Assert.AreEqual(2,config.Sources[0].Records.First().Rows[0].Columns.Count);
            Assert.AreEqual(2,config.Sources[0].Records.First().Rows[1].Columns.Count);
            Assert.AreEqual("test column 1",config.Sources[0].Records.First().Rows.First().Columns.First().Name);
            Assert.AreEqual(ColumnType.String,config.Sources[0].Records.First().Rows.First().Columns.First().Type);
            Assert.AreEqual("test column 4",config.Sources[0].Records.First().Rows.Last().Columns.Last().Name);
            Assert.AreEqual(ColumnType.Double,config.Sources[0].Records.First().Rows.Last().Columns.Last().Type);
        }
    }
}