using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Interfaces;
using Interfaces.Configuration;
using Newtonsoft.Json.Converters;

namespace Importer.Writers
{
    public static class FileWriter
    {
        public static Action<Func<IDataRow>> ConfigureWriters(this IContext context)
        {
            var writers = context.Config.Targets.GetNextFunc().ConfigureWriter(context.Log);

            return source =>
            {
                while (true)
                {
                    var row = source();
                    for (var i = 0; i < writers.Count; i++)
                    {
                        writers[i](row);
                    }

                    if (row == null)
                    {
                        return;
                    }
                }
            };
        }

        private static IList<Action<IDataRow>> ConfigureWriter(this Func<IFile> file, Interfaces.ILog log)
        {
            var writers = new List<Action<IDataRow>>();
            IFile fileConfig;
            while ((fileConfig = file()) != null)
            {
                if (!fileConfig.Disabled)
                {
                    var writer = fileConfig.GetWriter(log).GroupRecords(fileConfig,log);
                    if (writer != null)
                    {
                        writers.Add(writer);
                    }
                }
            }

            return writers;
        }

        private static Action<IDataRow> GetWriter(this IFile file, Interfaces.ILog log)
        {
            var stream = file.GetWriterStreams(log);
            var rowCount = (long) 0;
            if (stream != null)
            {
                switch (file.Format)
                {
                    case FileFormat.CSV:
                        var writerStream = stream.GetCsvWriter(file, log);
                        return row =>
                        {
                            if (row == null)
                            {
                                log?.Info(string.Format(
                                    Localization.GetLocalizationString("{0} line(s) written to {1}"),
                                    rowCount, file.Name));
                                stream.Flush();
                                return;
                            }

                            rowCount = writerStream(row);
                        };
                }
            }
            return row => { };
        }

        private static StreamWriter GetWriterStreams(this IFile file, Interfaces.ILog log)
        {
            var streams = new List<Stream>();
            if (file.MultipleMedia?.Count > 0)
            {
                streams = file.MultipleMedia.Where(x=>!x.Disabled).Select(x => x.GetWriterStream(file,log)).ToList();
            }
            else if(!file.Media.Disabled)
            {
                streams.Add(file.Media.GetWriterStream(file,log));
            }
            return new StreamWriter(new WriteStreamProxy(streams),Encoding.UTF8);
        }

        private static Stream GetWriterStream(this IFileMedia media, IFileConfiguration fileConfig, Interfaces.ILog log)
        {
            switch (media.MediaType)
            {
                case MediaType.Local:
                    return media.GetLocalStream(log);
                case MediaType.Wave:
                    return media.GetWaveStreamWriter(fileConfig,log);
            }

            return null;
        }

        private static Stream GetLocalStream(this IFileMedia media, Interfaces.ILog log)
        {
            if (File.Exists(media.Path) && media.Operation!=DataOperation.Append)
            {
                try
                {
                    File.Delete(media.Path);
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                    return null;
                }
            }

            return File.Open(media.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }
    }
}