using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator.Lexer;

namespace Parser
{
    public interface IParser
    {
        bool CheckSyntax(IEnumerable<Token> tokens);
    }
}
