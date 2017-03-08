using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser
{
    public interface IParser
    {
        bool CheckSyntax(IEnumerable<Token> tokens);
    }
}