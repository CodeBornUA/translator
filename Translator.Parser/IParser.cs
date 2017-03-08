using System.Collections.Generic;
using Translator.Lexer;

namespace Parser
{
    public interface IParser
    {
        bool CheckSyntax(IEnumerable<Token> tokens);
    }
}
