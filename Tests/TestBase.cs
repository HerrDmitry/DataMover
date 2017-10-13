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
    }
}