using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class UnconditionalJumpOperation : Token, IOperation
    {
        public override TokenType Type { get; set; }

        public void Execute(ExecutorContext executorContext)
        {
            var label = executorContext.Stack.Pop() as LabelToken;

            for (var i = 0; i < executorContext.Prn.Count - 1; i++)
            {
                var token = executorContext.Prn[i];
                var nextToken = executorContext.Prn[i + 1];
                if (token == label && nextToken.Substring == ":")
                {
                    executorContext.NextPosition = i + 2;
                    return;
                }
            }
        }
    }
}
