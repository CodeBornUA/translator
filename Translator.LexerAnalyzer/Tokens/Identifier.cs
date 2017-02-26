using System;

namespace Translator.Lexer
{
    public class Identifier : StringToken, IEquatable<Identifier>
    {

        public override TokenType Type => TokenType.Identifier;

        public string Name { get; }

        public Identifier(string name)
        {
            Name = name;
        }

        public bool Equals(Identifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Identifier) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Identifier left, Identifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Identifier left, Identifier right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Name;
        }

        public override string Substring => Name;
    }
}
