using System;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Configuration
{
    public static class ConfigurationExtensions
    {
        public static Func<IDataRow> GetDataReader(this IContext context)
        {
            var reader = context.Config.Sources.ConfigureReaders();
            return () => reader();
        }

        public static Func<IDataRow, bool> GetDataWriter(this IContext context)
        {
            return row =>
            {
                if (row == null)
                {
                    return false;
                }
                
                return true;
            };
        }

        public static IContext GetContext(this IConfiguration configuration)
        {
            return new Context{Config = configuration, Log = configuration.GetLogger()};
        }

        private static Interfaces.ILog GetLogger(this IConfiguration config)
        {
            return null;
        }
    }
}