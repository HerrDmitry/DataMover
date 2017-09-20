namespace Interfaces
{
    public interface IContext
    {
        IConfiguration config { get; }
        ILog log { get; }
    }
}