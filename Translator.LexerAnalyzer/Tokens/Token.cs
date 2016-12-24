using System;

namespace Translator.Lexer
{
    public abstract class Token : ICloneable
    {
        public int? TokenIndex { get; protected internal set; }
        public int? Index { get; protected internal set; }

        public abstract TokenType Type { get; set; }

        public virtual string Substring { get; set; }

        public int Line { get; protected internal set; }

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
