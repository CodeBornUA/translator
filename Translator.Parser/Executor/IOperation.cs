using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    internal interface IOperation
    {
        void Execute(Stack<Token> stack, VariableStore variableStore, IList<Token> prn, ref int position);
    }
}
