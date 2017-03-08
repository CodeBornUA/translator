using System;
using System.Collections.Generic;
using Translator.Lexer;

namespace Parser.Precedence
{
    public class GrammarReplaceRule
    {
        public PrecedenceParser.TokenEnum Token { get; }
        public CompositeToken CompositeToken { get; }

        public Action<List<Token>, List<Token>> OnReplaceAction { get; set; }

        public GrammarReplaceRule(PrecedenceParser.TokenEnum token, CompositeToken compositeToken)
        {
            Token = token;
            CompositeToken = compositeToken;
        }
    }
}