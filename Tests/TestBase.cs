using System;
using System.Diagnostics;
using System.IO;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public abstract class TestBase
    {
        private Stopwatch watch;

        [TestInitialize]
        public void Initialize()
        {
            watch=new Stopwatch();
            watch.Start();
        }

        [TestCleanup]
        public void Cleanup()
        {
            watch.Stop();
            Console.WriteLine($"Test finished in {watch.ElapsedMilliseconds}ms");
        }

        protected MemoryStream GetStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


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
}