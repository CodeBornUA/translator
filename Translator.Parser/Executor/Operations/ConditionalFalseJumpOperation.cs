using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class ConditionalFalseJumpOperation : Token, IOperation
    {
        public void Execute(Stack<Token> stack, VariableStore variableStore, IList<Token> prn, ref int position)
        {
            var label = stack.Pop() as LabelToken;

            var condition = stack.Pop();
            var conditionFloat = (condition as ConstantToken<float>)?.Value ??
                                    variableStore[condition as IdentifierToken].Value;

            if (conditionFloat == 0)
            {
                for (var i = 0; i < prn.Count - 1; i++)
                {
                    var token = prn[i];
                    var nextToken = prn[i + 1];
                    if (token == label && nextToken.Substring == ":")
                    {
                        position = i + 2;
                    }
                }
            }
        }

        public override TokenType Type { get; set; }
    }
}
