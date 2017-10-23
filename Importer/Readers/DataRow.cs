using System.Collections.Generic;
using Interfaces;

namespace Importer.Readers
{
	internal class DataRow : IDataRow
	{
		public DataRow(IDictionary<string, IValue> columns, string error, long rowNumber,
			long rawLineNumber, string sourcePath)
		{
			this.Columns = columns;
			this.Error = error;
			this.RowNumber = rowNumber;
			this.RawLineNumber = rawLineNumber;
			this.SourcePath = sourcePath;
		}

		public IDictionary<string, IValue> Columns { get; }
		public string Error { get; }
		public long RowNumber { get; }
		public long RawLineNumber { get; }

		public IValue this[string key] => this.Columns.TryGetValueDefault(key);
		public string SourcePath { get; }
	}
}