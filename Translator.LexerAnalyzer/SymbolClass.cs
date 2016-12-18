using System;
using System.Collections.Generic;
using System.Linq;

namespace Translator.Lexer
{
    public class SymbolClass : IEquatable<SymbolClass>
    {
        public Class Class { get; set; }

        public IList<char> Symbols { get; set; }

        public static SymbolClass operator |(SymbolClass first, SymbolClass second)
        {
            return new SymbolClass()
            {
                Class = first.Class | second.Class,
                Symbols = first.Symbols.Union(second.Symbols).ToList()
            };
        }

        public bool Equals(SymbolClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Class == other.Class;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SymbolClass) obj);
        }

        public override int GetHashCode()
        {
            return (int) Class;
        }

        public static bool operator ==(SymbolClass left, SymbolClass right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SymbolClass left, SymbolClass right)
        {
            return !Equals(left, right);
        }
    }

    [Flags]
    public enum Class
    {
        Letter = 1 << 0,
        Digit = 1 << 1,
        Splitter = 1 << 2,
        Operator = 1 << 3,
        Point = 1 << 4,
        Less = 1 << 5,
        Greater = 1 << 6,
        Equal = 1 << 7,
        Exclamation = 1 << 8,
        Colon = 1 << 9,
        Space = 1 << 10,
        Comma = 1 << 11,
        Hypen = 1 << 12
    }

    public struct Symbol : IEquatable<Symbol>
    {
        public static readonly Symbol Letter = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Letter} };
        public static readonly Symbol Digit = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Digit} };
        public static readonly Symbol Splitter = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Splitter} };
        public static readonly Symbol Operator = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Operator} };
        public static readonly Symbol Point = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Point} };
        public static readonly Symbol Less = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Less} };
        public static readonly Symbol Greater = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Greater} };
        public static readonly Symbol Equal = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Equal} };
        public static readonly Symbol Exclamation = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Exclamation} };
        public static readonly Symbol Colon = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Colon} };
        public static readonly Symbol Space = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Space} };
        public static readonly Symbol Comma = new Symbol() {Class = new SymbolClass() {Class = Translator.Lexer.Class.Comma} };
        public static readonly Symbol Hypen = new Symbol() { Class = new SymbolClass() { Class = Translator.Lexer.Class.Hypen } };

        public Symbol(char? symbol = null, IList<SymbolClass> classes = null)
        {
            Value = symbol;
            Class = symbol != null ? classes?.FirstOrDefault(x => x.Symbols.Contains(symbol.Value)) : null;
        }

        public SymbolClass Class { get; set; }

        public char? Value { get; set; }

        public bool Equals(Symbol other)
        {
            return Equals(Class, other.Class);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Symbol && Equals((Symbol) obj);
        }

        public override int GetHashCode()
        {
            return (Class != null ? Class.GetHashCode() : 0);
        }

        public static bool operator ==(Symbol left, Symbol right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Symbol left, Symbol right)
        {
            return !left.Equals(right);
        }

        public static Symbol operator |(Symbol first, Symbol second)
        {
            return new Symbol()
            {
                Class = first.Class | second.Class
            };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}