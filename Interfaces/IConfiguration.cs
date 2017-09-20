namespace Interfaces
{
    public interface IConfiguration
    {
        object GetValue(string key);
    }
}