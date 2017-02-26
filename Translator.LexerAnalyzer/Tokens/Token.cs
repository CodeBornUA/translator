using System;

namespace Translator.Lexer
{
    public abstract class Token : ICloneable, IEquatable<Token>
    {
        public int? TokenIndex { get; protected internal set; }
        public int? Index { get; protected internal set; }

        public abstract TokenType Type { get; set; }

        public virtual string Substring { get; set; }

        public int Line { get; set; }

        public bool Equals(Token other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Substring, other.Substring);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Token) obj);
        }

        public override int GetHashCode()
        {
            return (Substring != null ? Substring.GetHashCode() : 0);
        }

        public static bool operator ==(Token left, Token right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !Equals(left, right);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public enum TokenType
    {
        Empty,
        Identifier,
        Constant, 
        Label,
        Keyword,
        Operator,
        Unknown,
        Composite,
        Axiom,
        Nonterminal
    }
}
