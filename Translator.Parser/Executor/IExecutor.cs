using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public interface IExecutor
    {
        void Execute(IList<Token> tokenSequence, VariableStore variables, IList<LabelToken> labels, params string[] args);
    }
}
