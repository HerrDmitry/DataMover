using System.Text;

namespace Interfaces
{
    public interface IValue
    {
        string ToString(string format);
        string ToString();
        string GetError();
        bool IsNull { get; }
    }

    public interface IValue<out T> : IValue
    {
        T GetValue();
    }
}