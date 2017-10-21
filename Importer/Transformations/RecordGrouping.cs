using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Configuration;

namespace Importer
{
	public static partial class Transformations
	{
		public static Func<IDataRow> GetRecordGroupingFunc(this Func<IDataRow> getLineFunc, IFileConfiguration fileConfig,
			Interfaces.ILog logger)
		{
			var getRow = getLineFunc.GroupRowsFunc(fileConfig.Rows[0], logger);
			return () => getRow();
		}

		private static Func<IDataRow> GroupRowsFunc(this Func<IDataRow> rowSource, IRow rowConfig,ILog log)
		{
			var groupColumns = rowConfig.Columns.Where(x => x.GroupKey).Reverse().ToList();
			if (groupColumns.Count == 0)
			{
				return rowSource;
			}
			
			List<IDataRow> rows = new List<IDataRow>();;
			var dict=groupColumns.Count>1?new Dictionary<string,IDictionary>():(IDictionary)new Dictionary<string, IDataRow>();
			var addRowToGroup = rows.AddRowToGroupFunc(groupColumns, rowConfig, log);
			var index = -1;
			return () =>
			{
				if (index==-1)
				{
					index = 0;
					IDataRow row;
					while ((row = rowSource()) != null)
					{
						addRowToGroup(dict, row);
					}
				}

				return index < rows.Count ? rows[index++] : null;
			};
		}

		private static Action<IDictionary,IDataRow> AddRowToGroupFunc(this List<IDataRow> rows,  IList<IColumn> fieldNames, IRow rowConfig, ILog logger)
		{
			var fieldName = fieldNames.Last();
			Action<IDictionary,IDataRow> addChild;
			if (fieldNames.Count > 1)
			{
				addChild = rows.AddRowToGroupFunc(fieldNames.Take(fieldNames.Count - 1).ToList(), rowConfig, logger);
				return (dict, row) =>
				{
					var d = dict as Dictionary<string, IDictionary>;
					var field = row[fieldName.Name];
					var childDict = d.TryGetValueDefault(field.Source);
					if (childDict == null)
					{
						childDict = fieldNames.Count > 2
							? new Dictionary<string, IDictionary>()
							: (IDictionary) new Dictionary<string, IDataRow>();
						dict[field.Source] = childDict;
					}
					addChild(childDict,row);
				};
			}
			
			return (dict, row) =>
			{
				var d = dict as Dictionary<string, IDataRow>;
				if (dict == null)
				{
					throw new ImporterException("Incorrect grouping configuration, expected Dictionary<string,IDataRow>");
				}
				var field = row[fieldName.Name];
				var dictRow = d.TryGetValueDefault(field.Source);
				if (dictRow == null)
				{
					d[fieldName.Name] = row;
					rows.Add(row);
				}
				else
				{
					foreach (var c in rowConfig.Columns.Where(x => x.GroupKey == false))
					{
						dictRow[c.Name]?.Update(c.AggregateMethod,row[c.Name]);
					}
				}
			};
		}
	}
}