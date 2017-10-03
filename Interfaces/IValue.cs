using System.Text;

namespace Interfaces
{
    public interface IValue
    {
        string ToString(string format);
        string ToString();
        string GetError();
    }

    public interface IValue<out T> : IValue
    {
        T GetValue();
    }
}