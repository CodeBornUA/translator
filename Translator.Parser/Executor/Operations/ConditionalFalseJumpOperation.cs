using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class ConditionalFalseJumpOperation : Token, IOperation
    {
        public void Execute(ExecutorContext executorContext)
        {
            var label = executorContext.Stack.Pop() as LabelToken;

            var condition = executorContext.Stack.Pop();
            var conditionFloat = (condition as ConstantToken<float>)?.Value ??
                                    executorContext.Store[condition as IdentifierToken].Value;

            if (conditionFloat == 0)
            {
                for (var i = 0; i < executorContext.Prn.Count - 1; i++)
                {
                    var token = executorContext.Prn[i];
                    var nextToken = executorContext.Prn[i + 1];
                    if (token == label && nextToken.Substring == ":")
                    {
                        executorContext.NextPosition = i + 1;
                        return;
                    }
                }
            }
        }

        public override TokenType Type { get; set; }
    }
}
