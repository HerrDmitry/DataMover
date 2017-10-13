using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using Interfaces;
using Interfaces.Configuration;

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
            IFile fileMedia;
            while ((fileMedia = file()) != null)
            {
                var writer = fileMedia.GetWriter(log);
                if (writer != null)
                {
                    writers.Add(writer);
                }
            }

            return writers;
        }

        private static Action<IDataRow> GetWriter(this IFile file, Interfaces.ILog log)
        {
            var stream = file.GetWriterStream(log);
            var rowCount = (long) 0;
            if (stream != null)
            {
                switch (file.Format)
                {
                    case FileFormat.CSV:
                        var writerStream = stream.WriteCsv(file, log);
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

        private static StreamWriter GetWriterStream(this IFileMedia media, Interfaces.ILog log)
        {
            switch (media.MediaType)
            {
                case MediaType.Local:
                    return media.GetLocalStream(log);
            }
            return null;
        }

        private static StreamWriter GetLocalStream(this IFileMedia media, Interfaces.ILog log)
        {
            if (File.Exists(media.Path) && !media.AppendToExisting)
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

            return new StreamWriter(File.Open(media.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read));
        }
    }
}