using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interfaces;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<Func<StringBuilder>> CsvReader(this Stream stream, Func<string,object> getValue)
        {
            var readNext=new StreamReader(stream).BufferedRead();
            var d = getValue("delimiter") as char?;
            var q = getValue("qualifier") as char?;
            var delimiter = d ?? ',';
            var qualifier = q ?? '"';
            var locker = new object();
            return () =>
            {
                lock (locker)
                {
                    var columns = new Queue<StringBuilder>();
                    var endOfLine = false;
                    int c;
                    while ((c = readNext()) >= 0 && (c == '\n' || c == '\r')) ;
                    if (c < 0)
                    {
                        return null;
                    }
                    var isQualified = false;
                    var isRowDone = false;
                    var column=new StringBuilder();
                    while (c>=0)
                    {
                        if (c == qualifier)
                        {
                            isQualified = !isQualified;
                        }
                        if (c == delimiter && !isQualified)
                        {
                            columns.Enqueue(column);
                            column=new StringBuilder();
                        }
                        if ((c == '\n' || c == '\r') && !isQualified)
                        {
                            columns.Enqueue(column);
                            column = null;
                            break;
                        }
                        c = readNext();
                    }
                    if (c == -1 && column != null)
                    {
                        columns.Enqueue(column);
                    }

                    return () => columns.Count>0?columns.Dequeue():null;
                }
            };
        }
    }
}