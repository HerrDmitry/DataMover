using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Readers
{
	public partial class Readers
	{
		public static Func<ISourceRow> FixedWidthReader(this Func<int> readNext, ISourceFileContext context, Interfaces.ILog logger)
		{
			if (context?.FileMedia==null)
			{
				var msg = Localization.GetLocalizationString("Could not get Source Configuration...");
				logger?.Fatal(msg);
				throw new ArgumentException(msg);
			}
			var locker = new object();
			long rowCount = 0;
			logger?.Info(string.Format(Localization.GetLocalizationString("Loading data from {0}..."), context.FileConfiguration.Name));
			var lineReaders = context.FileConfiguration.Rows.Select(x => x.GetLineFunc(context.FileConfiguration)).ToList();

			return () =>
			{
				lock (locker)
				{
					var result = new List<ISourceField>();
					foreach (var reader in lineReaders)
					{
						result.AddRange(reader(readNext));
					}
					return result.Count > 0 ? new SourceRow {Context = context, Fields = result} : null;
				}
			};
		}

		private static Func<Func<int>, List<ISourceField>> GetLineFunc(this IRow row, IFixedWidthFile fileConfig)
		{
			var lineLength = row.Columns.Sum(x => x.Width);
			Func<List<ISourceField>, Action<int>> getAppender = result =>
			{
				var currentValue = new StringBuilder();
				var columnIndex = 0;
				return c =>
				{
					currentValue.Append((char) c);
					if (row.Columns[columnIndex].Width == currentValue.Length)
					{
						result.Add(new SourceField(currentValue.ToString()));
						currentValue = new StringBuilder();
						columnIndex++;
					}
				};
			};
			if (fileConfig.HasLineDelimiters)
			{
				return sourceFunc =>
				{
					var result = new List<ISourceField>();
					var append = getAppender(result);
					int v;
					int cIndex = 0;
					while ((v = sourceFunc()) >= 0 && (v == '\n' || v == '\r')) ;
					if (v >= 0)
					{
						cIndex++;
						append(v);
					}

					while (cIndex < lineLength && (v = sourceFunc()) >= 0)
					{
						cIndex++;
						append(v);
					}
					while ((v = sourceFunc()) >= 0 && v != '\n' && v != '\r') ;

					return result;
				};
			}

			return sourceFunc =>
			{
				var result = new List<ISourceField>();
				var append = getAppender(result);
				int v;
				for (var i = 0; i < lineLength && (v = sourceFunc()) >= 0; i++)
				{
					append((char) v);
				}

				return result;
			};
		}
	}
}