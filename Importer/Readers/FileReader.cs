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
        public static Func<int> BufferedRead (this ISourceFileContext r, Interfaces.ILog logger)
        {
            if (r?.Stream == null)
            {
                throw new ImporterArgumentOutOfRangeException(Localization.GetLocalizationString("Source stream cannot be null"));
            }
            var buff = new char[60];
            var position = 0;
            var length = 0;
            var rd = r.Stream;
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
                    if (rd==null || rd.EndOfStream)
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
            var nextSource = context.Config.Sources.Where(x=>!x.Disabled).GetNextFunc();
            var source = nextSource?.Invoke();
            var sourceStreamFunc = source?.Media?.GetSourceStream(source, context.Log);
            var sourceStream = sourceStreamFunc?.Invoke();
            var readerFunc = source.GetStreamReader(context.Log);
            var fileCount = 0;
            return () =>
            {
                while (sourceStream?.Stream != null)
                {
                    var reader = readerFunc?.Invoke(sourceStream);
                    sourceStream = sourceStreamFunc?.Invoke();
                    if (sourceStream == null)
                    {
                        source = nextSource();
                        readerFunc = source?.GetStreamReader(context.Log);
                        sourceStreamFunc = source?.Media.GetSourceStream(source, context.Log);
                        sourceStream = sourceStreamFunc?.Invoke();
                    }
                    
                    fileCount++;
                    return reader;
                }

                context.Log?.Info(string.Format(Localization.GetLocalizationString("Loaded {0} file(s)"), fileCount));
                return null;
            };
        }
        private static Func<ISourceFileContext, Func<IDataRow>> GetStreamReader(this IFileConfiguration fileConfig, Interfaces.ILog logger)
        {
            switch (fileConfig.Format)
            {
                case FileFormat.Fixed:
                    return context => context?.BufferedRead(logger).FixedWidthReader(context, logger).ParseData(context.FileConfiguration, logger);
                case FileFormat.CSV:
                    return context => context?.BufferedRead(logger).CsvReader(context, logger).ParseData(context.FileConfiguration, logger);
                default:
                    return null;
            }
        }

        private static Func<ISourceFileContext> GetSourceStream(this IFileMedia fileMedia, IFileConfiguration fileConfig, Interfaces.ILog logger)
        {
            logger?.Debug($"Getting source stream(s) for media \"{fileMedia.MediaType.ToString()}\" - \"{fileMedia.Path}\"");
            switch (fileMedia.MediaType)
            {
                case MediaType.Local:
                    var directoryName = Path.GetDirectoryName(Path.GetFullPath(fileMedia.Path));
                    logger?.Debug($"Searching for local file(s) \"{Path.GetFullPath(fileMedia.Path)}\"");
                    var files = Directory.EnumerateFiles(directoryName, Path.GetFileName(fileMedia.Path),
                        fileMedia.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).OrderBy(x=>x).ToList();
                    logger?.Debug($"Found {files.Count} file(s) matching the pattern.");
                    foreach (var file in files)
                    {
                        logger?.Debug(file);
                    }
                     
                    var getFile = files.GetNextFunc();

                    return () =>
                    {
                        var file = getFile();
                        return new SourceFileContext
                        {
                            SourcePath = file,
                            Stream = file.GetLocalFileStream(logger),
                            FileMedia = fileMedia,
                            FileConfiguration = fileConfig
                        };
                    };
            }
            return null;
        }

        private static StreamReader GetLocalFileStream(this string filePath, Interfaces.ILog logger)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }
            
            logger.Debug(string.Format(Localization.GetLocalizationString("Opening source file \"{0}\""),
                filePath));
            return new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }
    }
}