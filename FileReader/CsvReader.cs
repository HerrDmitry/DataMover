using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interfaces;

namespace FileReader
{
    
    public static class R
    {
        public static T GetNext<T>(Func<T> r) {
            return r();
        }

        public static string GetNextColumn(this Func<string> r)
        {
            return r();
        }

        public static Func<string> GetNextRow(this Func<Func<string>> r)
        {
            return r();
        }
    }
    
    public class CsvReader
    {
        private StreamReader reader;
        private char delimiter=',';
        private char qualifier='"';
        private Func<StringBuilder> GetNextColumn = () =>
        {
            return new StringBuilder();
        };
        public Func<Func<StringBuilder>> Reader(Stream stream, Func<string,object> getValue)
        {
            this.reader=new StreamReader(stream);
            var d = getValue("delimiter") as char?;
            var q = getValue("qualifier") as char?;
            delimiter = d ?? delimiter;
            qualifier = q ?? qualifier;
            var locker = new object();
            return () =>
            {
                lock (locker)
                {
                    var columns = new Queue<StringBuilder>();
                    var endOfLine = false;
                    char c='\n';
                    while (!this.reader.EndOfStream && (c=='\n'|| c=='\r'))
                    {
                        c = (char) this.reader.Read();
                    }
                    
                    return () =>
                    {
                        return columns.Dequeue();
                    };
                }
            };
        }
    }
}