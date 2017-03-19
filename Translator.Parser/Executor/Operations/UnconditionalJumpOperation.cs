using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class UnconditionalJumpOperation : Token, IOperation
    {
        public override TokenType Type { get; set; }

        public void Execute(Stack<Token> stack, VariableStore variableStore, IList<Token> prn, ref int position)
        {
            var label = stack.Pop() as LabelToken;

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
}
