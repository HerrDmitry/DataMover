using System;
using System.Diagnostics;
using System.IO;
using FileReader;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<StreamReader, Func<int>> read = (r) =>
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
            };
            Func<StreamReader, Func<int>> read1 = (r) =>
            {
                var rd = r;

                return () =>
                {
                        if (rd.EndOfStream)
                        {
                            return -1;
                        }
                    return rd.Read();
                };
            };

            if (args.Length == 0)
            {
                Console.WriteLine("oops");
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine(string.Format("{0} not found", args[0]));
            }
            var buffer = new char[655350];
            var watch = Stopwatch.StartNew();
            var reader = new StreamReader(File.Open(args[0],FileMode.Open,FileAccess.Read,FileShare.Read));
            while (!reader.EndOfStream) reader.ReadBlock(buffer, 0, 655350);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch=Stopwatch.StartNew();
            reader = new StreamReader(File.Open(args[0],FileMode.Open,FileAccess.Read,FileShare.Read));;
            var rdFunc = read(reader);
            while (rdFunc()> -1);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch=Stopwatch.StartNew();
            reader = new StreamReader(File.Open(args[0],FileMode.Open,FileAccess.Read,FileShare.Read));
            rdFunc = read1(reader);
            while (rdFunc()> -1);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch=Stopwatch.StartNew();
            reader = new StreamReader(File.Open(args[0],FileMode.Open,FileAccess.Read,FileShare.Read));
            var nextRow = reader.CsvReader(key =>
            {
                switch (key)
                {
                    case "delimiter":
                        return ',';
                    case "qualifier":
                        return '\'';
                    default:
                        return null;
                }
            });
            var rows = 0;
            var nextColumn = nextRow();
            while (nextColumn != null)
            {
                rows++;
                while (nextColumn() != null) ;
                nextColumn = nextRow();
            }
            watch.Stop();
            Console.WriteLine(rows);
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.WriteLine("Hello World!");
            
        }
    }
}