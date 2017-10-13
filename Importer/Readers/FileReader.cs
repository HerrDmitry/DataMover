using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Readers
{
    public static partial class Readers
    {
        public static Func<int> BufferedRead (this StreamReader r, Interfaces.ILog logger)
        {
            var buff = new char[60];
            var position = 0;
            var length = 0;
            var rd = r;
            var readChars = (long) 0;
            var resultMessage = new Action(() =>
            {
                if (readChars > 0)
                {
                    logger?.Debug($"Loaded {readChars} characters");
                }
            });
            return () =>
            {
                if (position >= length)
                {
                    if (rd.EndOfStream)
                    {
                        resultMessage();
                        readChars = 0;
                        return -1;
                    }
                    length = rd.ReadBlock(buff, 0, 5);
                    readChars += length;
                    position = 0;
                    if (length == 0)
                    {
                        return -1;
                    }
                }

                return buff[position++];
            };
        }

        public static Func<Func<IDataRow>> ConfigureReaders(this IContext context)
        {
            var nextSource = context.Config.Sources.GetNextFunc();
            var source = nextSource?.Invoke();
            var sourceStreamFunc = source?.GetSourceStream(context.Log);
            var sourceStream = sourceStreamFunc?.Invoke();
            var readerFunc = source.GetStreamReader(context.Log);
            var fileCount = 0;
            return () =>
            {
                while (sourceStream != null)
                {
                    var reader = readerFunc(sourceStream);
                    sourceStream = sourceStreamFunc();
                    if (sourceStream == null)
                    {
                        source = nextSource();
                        readerFunc = source.GetStreamReader(context.Log);
                        sourceStreamFunc = source?.GetSourceStream(context.Log);
                        sourceStream = sourceStreamFunc?.Invoke();
                    }
                    
                    fileCount++;
                    return reader;
                }

                context.Log?.Info(string.Format(Localization.GetLocalizationString("Loaded {0} file(s)"), fileCount));
                return null;
            };
        }

        private static Func<Stream,Func<IDataRow>> GetStreamReader(this IFile fileConfig, Interfaces.ILog logger)
        {
            if (fileConfig?.Format == FileFormat.CSV)
            {
                return stream => stream?.CsvReader(() => fileConfig, () => logger).ParseData(() => fileConfig, () => logger);
            }

            return null;
        }

        private static Func<Stream> GetSourceStream(this IFileMedia mediaInfo, Interfaces.ILog logger)
        {
            logger?.Debug($"Getting source stream(s) for media \"{mediaInfo.MediaType}\" - \"{mediaInfo.Path}\"");
            switch (mediaInfo.MediaType)
            {
                case MediaType.Local:
                    var directoryName = Path.GetDirectoryName(Path.GetFullPath(mediaInfo.Path));
                    logger?.Debug($"Searching for local file(s) \"{Path.GetFullPath(mediaInfo.Path)}\"");
                    var files = Directory.EnumerateFiles(directoryName, Path.GetFileName(mediaInfo.Path),
                        mediaInfo.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    logger?.Debug($"Found {files.Count()} file(s) matching the pattern.");
                    foreach (var file in files)
                    {
                        logger?.Debug(file);
                    }
                     
                    var getFile = files.GetNextFunc();

                    return () => getFile().GetLocalFileStream(logger);
            }
            return null;
        }

        private static Stream GetLocalFileStream(this string filePath, Interfaces.ILog logger)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }
            
            logger.Debug(string.Format(Localization.GetLocalizationString("Opening source file \"{0}\""),
                filePath));
            return File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}