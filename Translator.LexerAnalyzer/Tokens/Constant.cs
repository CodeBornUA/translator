using System;
using System.Globalization;
using System.Reflection;

namespace Translator.Lexer
{
    public sealed class Constant<T> : Token, IEquatable<Constant<T>> where T : struct, IConvertible
    {
        public override TokenType Type { get; set; } = TokenType.Constant;

        public T Value { get; }

        public Constant(string value)
        {
            if (!typeof(T).GetTypeInfo().IsPrimitive)
            {
                throw new InvalidOperationException("Constant MUST be a primitive type");
            }

            Escaped = value;
            Value = Parse(value);
        }

        public string Escaped { get; set; }

        public bool Equals(Constant<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Constant<T>) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Constant<T> left, Constant<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Constant<T> left, Constant<T> right)
        {
            return !Equals(left, right);
        }

        public static T Parse(string s)
        {
            return (T)(s as IConvertible).ToType(typeof(T), CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
