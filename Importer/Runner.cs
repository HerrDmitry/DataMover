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
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var dataReader = context.GetDataReader();
                context.GetDataWriter()(dataReader);
                watch.Stop();
                context.Log.Info(string.Format(Localization.GetLocalizationString("Import finished in {0}"),
                    watch.GetTime()));
            }
            finally
            {
                context.FinalizeImport();
            }

        }

        private static IConfiguration ApplyArguments(this IConfiguration config, string[] args)
        {
            if (config == null)
            {
                return null;
            }
            for (var i = 1; i < args.Length; i++)
            {
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
                            config.Credentials.UpdateCredentials(objectName, fieldName, argPair[1]);
                            config.Sources.UpdateConnectionCredentials(config.Credentials);
                            config.Targets.UpdateConnectionCredentials(config.Credentials);
                        }
                    }
                }
            }

            return config;
        }

        private static void UpdateConnectionCredentials(this IList<IFile> files, IDictionary<string,ICredentials> credentials)
        {
            foreach (var file in files)
            {
                if (file.Media != null)
                {
                    (file.Media as FileMedia).ConnectionCredentials = credentials.TryGetValueDefault(file.Media.Credentials);
                }
                if (file.MultipleMedia != null)
                {
                    foreach (var mm in file.MultipleMedia)
                    {
                        (mm as FileMedia).ConnectionCredentials = credentials.TryGetValueDefault(mm.Credentials);
                    }
                }
            }
        }

        private static void UpdateCredentials(this IDictionary<string, ICredentials> credentials, string name,
            string field,
            string value)
        {
            if (credentials.TryGetValueDefault(name) is Credentials c)
            {
                switch (field.ToUpper())
                {
                    case "LOGIN":
                        c.Login = value;
                        return;
                    case "PASSWORD":
                        c.Password = value;
                        return;
                    case "TOKEN":
                        c.Token = value;
                        return;
                    case "CLIENTID":
                        c.ClientId = value;
                        return;
                    case "CLIENTSECRET":
                        c.ClientSecret = value;
                        return;
                }
            }
        }

        private static void UpdateFileConfiguration(this IEnumerable<File> fileConfigs, string name, string field,
            string value)
        {
            foreach (var fileConfig in fileConfigs)
            {
                if (fileConfig.Name == name)
                {
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