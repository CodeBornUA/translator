using System;
using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public interface IExecutor
    {
        void Execute(Context context, params string[] args);

        event Action<string> Output;
        event Action<IList<Token>, int, Stack<Token>> ComputationStep;
    }

    public class Context
    {
        public Context(IList<Token> tokenSequence, VariableStore variables, IList<LabelToken> labels)
        {
            TokenSequence = tokenSequence;
            Variables = variables;
            Labels = labels;
        }

        public IList<Token> TokenSequence { get; private set; }
        public VariableStore Variables { get; private set; }
        public IList<LabelToken> Labels { get; private set; }
    }
}
