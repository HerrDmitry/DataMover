using Interfaces;
using Interfaces.Configuration;

namespace Importer
{
    public class Context : IContext
    {
        public IConfiguration Config { get; set; }
        public Interfaces.ILog Log { get; set; }
    }
}