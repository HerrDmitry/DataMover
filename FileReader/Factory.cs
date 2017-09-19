using System;
using Interfaces;

namespace FileReader
{
    public static class Factory
    {
        public static IStreamReader GetReader(Func<string> getReaderType)
        {
            switch (getReaderType())
            {
                case "csv":
                    return new CsvReader();
                default:
                    return null;
            }
        }
    }
}