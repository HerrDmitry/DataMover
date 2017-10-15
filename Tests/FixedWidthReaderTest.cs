using System;
using System.Collections.Generic;
using Importer;
using Importer.Configuration;
using Importer.Readers;
using Interfaces;
using Interfaces.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class FixedWidthReaderTest : TestBase
	{
		[TestMethod]
		public void TestFixedWidth()
		{
			var source = "\r1233211someextratext\r\n3211232";
			var stream = base.GetStreamFromString(source);
			var data = stream.BufferedRead(new ConsoleLogger()).FixedWidthReader(getConfig(), new ConsoleLogger())
				.ParseData(getConfig(), new ConsoleLogger());
			var row = data();
			Assert.IsNotNull(row);
			Console.WriteLine($":{row["c1"]}:{row["c2"]}:{row["c3"]}");
			Assert.AreEqual(1, ((IValue<long>)row["c3"]).GetValue());
			Assert.AreEqual("123", row["c1"].ToString());
			Assert.AreEqual("321", row["c2"].ToString());
			row = data();
			Assert.AreEqual(2, ((IValue<long>)row["c3"]).GetValue());
			Assert.AreEqual("123", row["c2"].ToString());
			Assert.AreEqual("321", row["c1"].ToString());
		}

		private IFile getConfig()
		{
			return new File
			{
				HasLineDelimiters = true,
				RowsInternal = new List<Row>
				{
					new Row
					{
						ColumnsInternal = new List<Column>
						{
							new Column {Name = "c1", Width = 3, Type=ColumnType.String},
							new Column {Name = "c2", Width = 3, Type=ColumnType.String},
							new Column {Name = "c3", Width = 1, Type=ColumnType.Integer}
						}
					}
				}
			};
		}
	}
}