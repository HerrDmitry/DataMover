namespace DataMover.Parsers
{
    public interface IParser{
		void Parse(string source);
		string ToString();
		string ToString(string format);
	}
    public interface IParser<T>:IParser
    {
		T Value { get; }
    }
}
