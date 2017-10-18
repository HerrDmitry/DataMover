using System;
using Interfaces;

namespace Importer
{
    public class ConsoleLogger :LoggerBase
    {
        public ConsoleLogger(LogLevel logLevel = LogLevel.Debug) : base(logLevel)
        {
        }

        public override Action Terminate => () => { };

        protected override void WriteLog(LogLevel level, string message)
        {
            var oldColor = Console.ForegroundColor;
            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}