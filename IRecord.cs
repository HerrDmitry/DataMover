using System.Collections.Generic;
using DataMover.Parsers;

namespace DataMover
{
    public interface IRecord:IEnumerable<IParser>,IDictionary<string,IParser>
    {
        void Parse(string source);
    }
}
