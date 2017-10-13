﻿using System;
using System.Linq;
using Importer.Readers;
using Importer.Writers;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Configuration
{
    public static class ConfigurationExtensions
    {
        public static Func<IDataRow> GetDataReader(this IContext context)
        {
            var dataReader=context.ConfigureReaders();
            var rowSource=dataReader?.Invoke();
            return () =>
            {
                while (rowSource != null)
                {
                    var row = rowSource();
                    if (!string.IsNullOrEmpty(row?.Error))
                    {
                        context.Log?.Error(string.Format(
                            Localization.GetLocalizationString("Could not parse line {0} - \"{1}\""), row.RowNumber,
                            string.Join(",", row.Columns.Values.Select(x => x.ToString()))));
                    }
                    if (row != null)
                    {
                        return row;
                    }

                    rowSource = dataReader();
                }

                return null;
            };
        }

        public static Action<Func<IDataRow>> GetDataWriter(this IContext context)
        {
            return context.ConfigureWriters();
        }

        public static IContext GetContext(this IConfiguration configuration)
        {
            return new Context{Config = configuration, Log = configuration.GetLogger()};
        }

        private static Interfaces.ILog GetLogger(this IConfiguration config)
        {
            return new ConsoleLogger();
        }
    }
}