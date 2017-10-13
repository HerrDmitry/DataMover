using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FileReader;
using Importer.Readers;
using Interfaces;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    Runner.RunImport(args[0]);
                }
                else
                {
                    Console.WriteLine(Localization.GetLocalizationString("Configuration file {0} not found"),Path.GetFullPath(args[0]));
                }
            }
            else
            {
                Console.WriteLine(Localization.GetLocalizationString("Missing configuration file name as first argument."));
            }
        }

        static void Test(string[] args)
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
                return;
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
            var nextRow = reader.CsvReader(() => new Configuration.File()
            {
                Delimiter = ";",
                Qualifier = "\""
            },()=>null);
            var rows = 0;
            IReadOnlyList<ISourceField> record;
            long colCount = 0;
            while ((record = nextRow()) != null)
            {
                rows++;
                colCount += record.Count;
            }
            watch.Stop();
            Console.WriteLine(rows);
            Console.WriteLine(colCount);
            Console.WriteLine(watch.ElapsedMilliseconds);
            Console.WriteLine("Hello World!");
            
        }
    }
}