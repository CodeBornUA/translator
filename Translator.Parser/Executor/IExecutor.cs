using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public interface IExecutor
    {
        void Execute(IList<Token> tokenSequence, params string[] args);
    }
}
