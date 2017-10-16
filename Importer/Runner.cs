using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Interfaces.Configuration;
using Newtonsoft.Json;
using Importer.Configuration;
using File = Importer.Configuration.File;

namespace Importer
{
    public static class Runner
    {
        public static void RunImport(string[] args)
        {
            var configurationFilePath=args[0];
            
            var configuration =
                JsonConvert.DeserializeObject<Configuration.Configuration>(System.IO.File.OpenText(configurationFilePath)
                    .ReadToEnd()).ApplyArguments(args);
            
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

        private static IConfiguration ApplyArguments(this IConfiguration config, string[] args)
        {
            if (config == null)
            {
                return null;
            }
            for (var i = 1; i < args.Length; i++)
            {
                bool isUpdated = false;
                var argPair = args[i].Split(":", StringSplitOptions.RemoveEmptyEntries);
                if (argPair.Length == 2)
                {
                    var namePair = argPair[0].Split(".");
                    if (namePair.Length > 1)
                    {
                        var objectName = string.Join(".", namePair.Take(namePair.Length - 1));
                        var fieldName = namePair.Last();

                        if (objectName.Length > 0)
                        {
                            config.Sources.Cast<File>().UpdateFileConfiguration(objectName,fieldName,argPair[1]);
                            config.Targets.Cast<File>().UpdateFileConfiguration(objectName,fieldName,argPair[1]);
                        }
                    }
                }
            }

            return config;
        }

        private static void UpdateFileConfiguration(this IEnumerable<File> fileConfigs, string name, string field,
            string value)
        {
            foreach (var fileConfig in fileConfigs)
            {
                if (fileConfig.Name == name)
                {
                    switch (field.ToUpper())
                    {
                        case "LOGIN":
                            fileConfig.Login = value;
                            return;
                        case "PASSWORD":
                            fileConfig.Password = value;
                            return;
                        case "TOKEN":
                            fileConfig.Token = value;
                            return;
                    }
                }
            }
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