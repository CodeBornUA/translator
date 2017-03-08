using System;
using System.Globalization;
using System.Reflection;

namespace Translator.LexerAnalyzer.Tokens
{
    public sealed class ConstantToken<T> : Token, IEquatable<ConstantToken<T>> where T : struct, IConvertible
    {
        public ConstantToken(string value)
        {
            if (!typeof(T).GetTypeInfo().IsPrimitive)
                throw new InvalidOperationException("Constant MUST be a primitive type");

            Escaped = value;
            Value = Parse(value);
        }

        public ConstantToken(T value)
        {
            if (!typeof(T).GetTypeInfo().IsPrimitive)
                throw new InvalidOperationException("Constant MUST be a primitive type");

            Escaped = value.ToString();
            Value = value;
        }

        public override TokenType Type { get; set; } = TokenType.Constant;

        public T Value { get; }

        public string Escaped { get; set; }

        public bool Equals(ConstantToken<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConstantToken<T>) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ConstantToken<T> left, ConstantToken<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ConstantToken<T> left, ConstantToken<T> right)
        {
            return !Equals(left, right);
        }

        public static T Parse(string s)
        {
            return (T) (s as IConvertible).ToType(typeof(T), CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}