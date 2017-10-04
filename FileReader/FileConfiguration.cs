using System.Collections.Generic;
using Interfaces;
using Interfaces.FileDefinition;

namespace FileReader
{
    public class FileConfiguration:IFile
    {
        public string Name { get; set; }
        public IList<IRecord> Records { get; set; }

        public FileConfiguration()
        {
            this.Records=new List<IRecord>();
        }

        public class FileRecordConfiguration : IRecord
        {
            public IList<IColumn> Columns { get; set; }

            public FileRecordConfiguration()
            {
                this.Columns=new List<IColumn>();
            }
        }

        public class FileColumnConfiguration : IColumn
        {
            public string Name { get; set; }
            public string Format { get; set; }
            public ColumnType Type { get; set; }
        }
    }
}