using System.Text;

namespace Interfaces
{
    public interface IValue
    {
        string ToString(string format);
        string ToString();
        string GetError();
        bool IsNull { get; }
        string Source { get; }
        void Update(IValue newValue);
        void Update(AggregateMethod method, IValue value);
    }

    public interface IValue<T> : IValue
    {
        T GetValue();
    }
}