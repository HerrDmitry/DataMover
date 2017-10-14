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
    }
}