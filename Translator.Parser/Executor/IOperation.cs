using System.Collections.Generic;
using Parser.Executor.Operations;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    internal interface IOperation
    {
        void Execute(ExecutorContext executorContext);
    }
}
