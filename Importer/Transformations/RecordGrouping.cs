using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Importer.Readers;
using Interfaces;
using Interfaces.Configuration;

namespace Importer
{
	public static partial class Transformations
	{
		public static Action<IDataRow> GroupRecords(this Action<IDataRow> writer, IFileConfiguration fileConfig, ILog logger)
		{
			return writer.GroupRecords(fileConfig.Rows[0], logger);
		}
		
		public static Action<IDataRow> GroupRecords(this Action<IDataRow> writer, IRow rowConfig, ILog logger)
		{
			var groupColumns = rowConfig.Columns.Where(x => x.GroupKey).Reverse().ToList();
			if (groupColumns.Count == 0)
			{
				return writer;
			}
			
			List<IDataRow> rows = new List<IDataRow>();;
			var dict=groupColumns.Count>1?new Dictionary<string,IDictionary>():(IDictionary)new Dictionary<string, IDataRow>();
			var addRowToGroup = rows.AddRowToGroupFunc(groupColumns, rowConfig, logger);
			return row =>
			{
				if (!string.IsNullOrWhiteSpace(row?.Error))
				{
					return;
				}
				if (row != null)
				{
					addRowToGroup(dict, row);
				}
				else
				{
					foreach (var r in rows)
					{
						writer(r);
					}
					writer(null);
				}
			};
		}

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
					var childDict = d.TryGetValueDefault(field.Source??"null");
					if (childDict == null)
					{
						childDict = fieldNames.Count > 2
							? new Dictionary<string, IDictionary>()
							: (IDictionary) new Dictionary<string, IDataRow>();
						dict[field.Source??"null"] = childDict;
					}
					addChild(childDict,row);
				};
			}
			
			return (dict, row) =>
			{
				if (!(dict is Dictionary<string, IDataRow> d))
				{
					throw new ImporterException("Incorrect grouping configuration, expected Dictionary<string,IDataRow>");
				}
				var field = row[fieldName.Name];
				var dictRow = d.TryGetValueDefault(field.Source??"null");
				if (dictRow == null)
				{
					var clone = row.CloneRow(rowConfig);
					d[field.Source??"null"] = clone;
					rows.Add(clone);
				}
				else
				{
					foreach (var c in rowConfig.Columns.Where(x => x.GroupKey == false))
					{
						dictRow[c.Alias??c.Name].Update(c.AggregateMethod,row[c.Name]);
					}
				}
			};
		}

		private static IDataRow CloneRow(this IDataRow row, IRow targetConfig)
		{
			var columns = new Dictionary<string, IValue>();
			foreach (var c in targetConfig.Columns)
			{
				var value = row[c.Name].CloneValue(c);
				columns[c.Alias??c.Name] = value;
			}

			return new DataRow(columns, row.Error, row.RowNumber, row.RawLineNumber, row.SourcePath);
		}

		private static IValue CloneValue(this IValue source, IColumn column)
		{
			switch (column.Type)
			{
				case ColumnType.Date:
					if (source.GetValueType() == typeof(DateTime))
					{
						return new ValueWrapper<DateTime>().Assign(source);
					}
					return column.GetDateTimeParser()(source.ToString());
				case ColumnType.Decimal:
				case ColumnType.Money:
					if (source.GetValueType() == typeof(decimal))
					{
						return new ValueWrapper<decimal>().Assign(source);
					}
					return column.GetDecimalParser()(source.ToString());
				case ColumnType.Integer:
					if (source.GetValueType() == typeof(long))
					{
						return new ValueWrapper<long>().Assign(source);
					}
					return column.GetIntegerParser()(source.ToString());
				case ColumnType.String:
					return new ValueWrapper<string>(source.ToString(column.Format),source.GetError(),source.IsNull,source.Source);
			}

			return null;
		}
	}
}