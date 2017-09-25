﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interfaces;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<Func<StringBuilder>> CsvReader(this Stream stream, Func<string, object> getValue)
        {
            return new StreamReader(stream).CsvReader(getValue);
        }

        public static Func<Func<StringBuilder>> CsvReader(this StreamReader stream, Func<string,object> getValue)
        {
            var readNext=stream.BufferedRead();
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
                    int c;
                    while ((c = readNext()) >= 0 && (c == '\n' || c == '\r')) ;
                    if (c < 0)
                    {
                        return null;
                    }
                    var isQualified = false;
                    var column=new StringBuilder();
                    while (c>=0)
                    {
                        if (c == qualifier)
                        {
                            isQualified = !isQualified;
                            if (isQualified && column.Length > 0)
                            {
                                column.Append((char)c);
                            }
                        }
                        else
                        if (c == delimiter && !isQualified)
                        {
                            columns.Enqueue(column);
                            column = new StringBuilder();
                        }
                        else
                        if ((c == '\n' || c == '\r') && !isQualified)
                        {
                            columns.Enqueue(column);
                            column = null;
                            break;
                        }
                        else
                        {
                            column.Append((char)c);
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