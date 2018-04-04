using System;

namespace RedisJsonClient
{
    public struct ConditionalValue<T>
    {
        public ConditionalValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public bool HasValue { get; }
        public T Value { get; } 

        public T GetValueOrDefault(T @default)
        {
            return HasValue ? Value : @default;
        }

        public static readonly ConditionalValue<T> NoValue = new ConditionalValue<T>();

        public static implicit operator bool(ConditionalValue<T> value)
        {
            return value.HasValue;
        }

        public static implicit operator T(ConditionalValue<T> value)
        {
            if (!value.HasValue)
                throw new ArgumentException("Argument has no value", nameof(value));
            return value.Value;
        }
    }

    public static class ConditionalValue
    {
        public static ConditionalValue<T> From<T>(T value)
        {
            return new ConditionalValue<T>(value);
        }
    }
}