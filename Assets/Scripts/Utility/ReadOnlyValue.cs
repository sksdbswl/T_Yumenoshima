using System;

namespace REIW
{
    public readonly struct ReadOnlyValue<T>
    {
        public T Value { get; }

        public ReadOnlyValue(T value)
        {
            Value = value;
        }

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override bool Equals(object obj) => obj is ReadOnlyValue<T> other && Equals(Value, other.Value);
        public override string ToString() => Value?.ToString() ?? string.Empty;

        public static implicit operator ReadOnlyValue<T>(T value) => new ReadOnlyValue<T>(value);
        public static implicit operator T(ReadOnlyValue<T> readOnly) => readOnly.Value;
    }
}
