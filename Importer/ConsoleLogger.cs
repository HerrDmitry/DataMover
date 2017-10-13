using System;
using Interfaces;

namespace Importer
{
    public class ConsoleLogger : ILog
    {
            
        public Action<string> Info => message => this.WriteLine(LogLevel.Info, message);
        public Action<string> Debug => message => this.WriteLine(LogLevel.Debug, message);
        public Action<string> Warning => message => this.WriteLine(LogLevel.Warning, message);
        public Action<string> Error => message => this.WriteLine(LogLevel.Error, message);
        public Action<string> Fatal => message => this.WriteLine(LogLevel.Fatal, message);

        private static object logLocker=new object();
        private void WriteLine(LogLevel level, string message)
        {
            lock (logLocker)
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
}