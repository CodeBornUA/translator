using System;

namespace Translator.LexerAnalyzer.Tokens
{
    public class IdentifierToken : StringToken, IEquatable<IdentifierToken>
    {
        public IdentifierToken(string name)
        {
            Name = name;
        }

        public override TokenType Type => TokenType.Identifier;

        public string Name { get; }

        public override string Substring => Name;

        public bool Equals(IdentifierToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IdentifierToken) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(IdentifierToken left, IdentifierToken right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentifierToken left, IdentifierToken right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}