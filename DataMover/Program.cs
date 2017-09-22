using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataMover.Configuration;
using DataMover.Loaders;
using DataMover.Logger;

namespace DataMover
{
    class Program
    {
        static void Main(string[] args)
        {
            DataMoverLog.InfoAsync("Starting the application");
            MigrationConfiguration configuration = null;
            if (args.Length > 0)
            {
                DataMoverLog.DebugAsync("Received parameters:");
                for (var i = 0; i < args.Length; i++)
                {
                    DataMoverLog.DebugAsync($"{i} - {args[i]}");
                }

                configuration = ConfigurationLoader.LoadConfiguration(args);
            }

            if (configuration == null)
            {
                DataMoverLog.ErrorAsync("Missing configuration.");
                PrintUsage();
            }
            else
            {
                var stream = System.IO.File.Open(configuration.Files[0].Path, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                var loader = new CsvLoader(configuration.Files[0].Name);
                foreach (var l in loader.ReadLines(stream))
                {
                }
            }
            Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
            DataMoverLog.Terminate();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine("DataMover /c:{configuration file} [/f:{name}={source/target file} /f:{name}={source/target file} ...]");
        }
    }
}
