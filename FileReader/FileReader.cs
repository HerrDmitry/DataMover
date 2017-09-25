using System;
using System.IO;

namespace FileReader
{
    public static partial class Readers
    {
        public static Func<int> BufferedRead (this StreamReader r)
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
        }    
    }
}