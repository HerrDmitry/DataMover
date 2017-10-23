using System;
using Interfaces;

namespace Importer.Readers
{
    internal class ValueWrapper<T> : IValue<T>
    {
        public ValueWrapper() : this(default(T), null, true, null)
        {
        }

        public ValueWrapper(T value, string error, bool isNull, string source)
        {
            this.Value = value;
            this.Error = error;
            this.IsNull = isNull;
            this.Source = source;
        }

        private T Value;
        private string Error;
        public string Source { get; private set; }

        public void Update(IValue newValue)
        {
            this.Update(AggregateMethod.Last, newValue);
        }

        public void Update(AggregateMethod method, IValue value)
        {
            if (value.IsNull)
            {
                return;
            }
            switch (method)
            {
                case AggregateMethod.Last:
                    if (typeof(T) == typeof(string))
                    {
                        (this as ValueWrapper<string>).Value = value.ToString();
                    }
                    else
                    {
                        this.Assign(value);
                    }
                    break;
                case AggregateMethod.Join:
                    if (typeof(T) == typeof(string))
                    {
                        var stringValue = value.ToString();
                        if (this.Value == null || (this.Value as string)?.Contains(stringValue) == false)
                        {
                            (this as ValueWrapper<string>).Value =
                                string.Concat((this.Value as string) ?? "", ",", stringValue);
                        }
                    }
                    break;
                case AggregateMethod.FullJoin:
                    if (typeof(T) == typeof(string))
                    {
                        var stringValue = value.ToString();
                        (this as ValueWrapper<string>).Value =
                            string.Concat((this.Value as string) ?? "", ",", stringValue);
                    }
                    break;
                case AggregateMethod.Sum:
                    if (typeof(T) == typeof(long))
                    {
                        (this as ValueWrapper<long>).Value += (value as IValue<long>)?.GetValue() ?? 0;
                    }
                    if (typeof(T) == typeof(decimal))
                    {
                        (this as ValueWrapper<decimal>).Value += (value as IValue<decimal>)?.GetValue() ?? 0;
                    }
                    break;
            }
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public T GetValue()
        {
            return this.Value;
        }

        public IValue Assign(IValue value)
        {
            this.Value = value is IValue<T> tValue
                ? tValue.GetValue()
                : throw new ImporterArgumentOutOfRangeException(
                    Localization.GetLocalizationString("Cannot assign \"{0}\" to \"{1}\"",
                        value.GetValueType().Name, typeof(T).Name));
            this.Source = value.Source;
            this.Error = value.GetError();
            this.IsNull = value.IsNull;
            return this;
        }

        public virtual string ToString(string format)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return (this as IValue<DateTime>)?.GetValue().ToString(format);
            }
            return Value?.ToString();
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string GetError()
        {
            return this.Error;
        }

        public bool IsNull { get; private set; }
    }
}