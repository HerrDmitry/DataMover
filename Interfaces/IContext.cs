using Interfaces.Configuration;

namespace Interfaces
{
    public interface IContext
    {
        IConfiguration Config { get; }
        ILog Log { get; }
    }
}