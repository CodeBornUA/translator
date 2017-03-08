using System.Collections.Generic;
using Translator.Lexer;

namespace Parser.Executor
{
    public static class PrnExpressionExecutor
    {
        public static float ComputeExpression(IList<Token> prn, IDictionary<Identifier, Constant<float>> identifierValues)
        {
            var stack = new Stack<Token>();

            foreach (var token in prn)
            {
                if (token is Identifier || token is Constant<float>)
                {
                    stack.Push(token);
                }

                if (token.Substring == "@")
                {
                    var operand = stack.Pop();

                    var floatOperand = (operand as Constant<float>)?.Value ?? identifierValues[operand as Identifier].Value;
                    stack.Push(new Constant<float>(-floatOperand));
                }

                if (token.Substring == "+" || token.Substring == "-" || token.Substring == "*" || token.Substring == "/")
                {
                    var operand2 = stack.Pop();
                    var operand1 = stack.Pop();

                    var floatOperand2 = (operand2 as Constant<float>)?.Value ?? identifierValues[operand2 as Identifier].Value;
                    var floatOperand1 = (operand1 as Constant<float>)?.Value ?? identifierValues[operand1 as Identifier].Value;

                    float localResult = 0;
                    switch (token.Substring)
                    {
                        case "*":
                            localResult = floatOperand1 * floatOperand2;
                            break;
                        case "/":
                            localResult = floatOperand1 / floatOperand2;
                            break;
                        case "+":
                            localResult = floatOperand1 + floatOperand2;
                            break;
                        case "-":
                            localResult = floatOperand1 - floatOperand2;
                            break;
                    }
                    stack.Push(new Constant<float>(localResult));
                }
            }

            var popped = stack.Pop();
            return (popped as Constant<float>)?.Value ?? identifierValues[popped as Identifier].Value;
        }
    }
}
