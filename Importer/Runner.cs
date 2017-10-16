using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json;
using Importer.Configuration;

namespace Importer
{
    public static class Runner
    {
        public static void RunImport(string[] args)
        {
            var configurationFilePath=args[0];
            
            var configuration =
                JsonConvert.DeserializeObject<Configuration.Configuration>(System.IO.File.OpenText(configurationFilePath)
                    .ReadToEnd());
            var context = configuration.GetContext();
            context.Log.Debug("Starting import...");
            var watch = new Stopwatch();
            watch.Start();
            context.GetDataWriter()(context.GetDataReader());
            watch.Stop();
            context.Log.Info(string.Format(Localization.GetLocalizationString("Import finished in {0}"),
                watch.GetTime()));
            context.FinalizeImport();
            
        }

        private static string GetTime(this Stopwatch watch)
        {
            var time = new StringBuilder();
            if (watch.Elapsed.TotalMinutes >= 1)
            {
                time.Append($"{(int)watch.Elapsed.TotalMinutes:D2}:");
            }
            time.Append($"{watch.Elapsed.Seconds:D2}.{watch.Elapsed.Milliseconds:D3}");
            return time.ToString();
        }
    }
}