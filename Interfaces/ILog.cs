namespace Interfaces
{
    public interface ILog
    {
        void Error(string message);
        void Warning(string message);
        void Info(string message);
        void Fatal(string message);
    }
}