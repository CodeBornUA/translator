using System.Collections.Generic;
using System.IO;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class ExecutorContext
    {
        public int? NextPosition { get; set; }
        public IList<Token> Prn { get; }
        public VariableStore Store { get; }
        public Stack<Token> Stack { get; }
        public Stream InputStream { get; set; }
        public Stream OutputStream { get; set; }

        public ExecutorContext(Stack<Token> stack, VariableStore variableStore, IList<Token> prn, int? nextPostition = null)
        {
            Stack = stack;
            Store = variableStore;
            Prn = prn;
            NextPosition = nextPostition;
        }
    }
}