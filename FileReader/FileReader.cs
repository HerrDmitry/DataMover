using System;
using System.Collections.Generic;
using System.IO;
using Interfaces;
using Interfaces.Configuration;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<int> BufferedRead (this StreamReader r)
        {
            var buff = new char[65535];
            var position = 0;
            var length = 0;
            var rd = r;
                
            return () =>
            {
                if (position >= length)
                {
                    if (rd.EndOfStream)
                    {
                        return -1;
                    }
                    length = rd.ReadBlock(buff, 0, 65535);
                    position = 0;
                    if (length == 0)
                    {
                        return -1;
                    }
                }

                return buff[position++];
            };
        }

        public static Func<IDataRow> ConfigureReaders(this IList<IFile> sources)
        {
            var sourcesEnumerator = sources.GetEnumerator();
            sourcesEnumerator.Reset();
            Func<Stream> sourceStream = null;
            return () =>
            {
                if (sourceStream != null)
                {
                    var stream = sourceStream();
                    if (stream != null)
                    {
                        return stream.GetStreamReader(sourcesEnumerator.Current);
                    }
                }
                sourceStream = sourcesEnumerator.Current.GetSourceStream();

            };
        }

        public static Func<IDataRow> GetStreamReader(this Stream stream, IFile fileConfig)
        {
        }

        public static Func<Stream> GetSourceStream(IFileMedia mediaInfo)
        {
            switch (mediaInfo.MediaType)
            {
                case MediaType.Local:
                    var directoryName = Path.GetDirectoryName(mediaInfo.Path);
                    var files = Directory.EnumerateFiles(directoryName, Path.GetFileName(mediaInfo.Path),
                        mediaInfo.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).GetEnumerator();
                    files.Reset();
                    return () => !files.MoveNext() ? null : File.Open(files.Current, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            return null;
        }
    }
}