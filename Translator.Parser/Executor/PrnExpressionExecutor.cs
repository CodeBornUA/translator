using System.Collections.Generic;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public static class PrnExpressionExecutor
    {
        public static float ComputeExpression(IList<Token> prn,
            IDictionary<IdentifierToken, ConstantToken<float>> identifierValues)
        {
            var stack = new Stack<Token>();

            foreach (var token in prn)
            {
                ProcessIndentifiersConstants(token, stack);

                ProcessUnarySubtraction(identifierValues, token, stack);

                ProcessArithmeticOperations(identifierValues, token, stack);
            }

            var popped = stack.Pop();
            return (popped as ConstantToken<float>)?.Value ?? identifierValues[popped as IdentifierToken].Value;
        }

        private static void ProcessArithmeticOperations(IDictionary<IdentifierToken, ConstantToken<float>> identifierValues,
            Token token, Stack<Token> stack)
        {
            if (token.Substring == "+" || token.Substring == "-" || token.Substring == "*" || token.Substring == "/")
            {
                var operand2 = stack.Pop();
                var operand1 = stack.Pop();

                var floatOperand2 = (operand2 as ConstantToken<float>)?.Value ??
                                    identifierValues[operand2 as IdentifierToken].Value;
                var floatOperand1 = (operand1 as ConstantToken<float>)?.Value ??
                                    identifierValues[operand1 as IdentifierToken].Value;

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
                stack.Push(new ConstantToken<float>(localResult));
            }
        }

        private static void ProcessIndentifiersConstants(Token token, Stack<Token> stack)
        {
            if (token is IdentifierToken || token is ConstantToken<float>)
                stack.Push(token);
        }

        private static void ProcessUnarySubtraction(IDictionary<IdentifierToken, ConstantToken<float>> identifierValues,
            Token token, Stack<Token> stack)
        {
            if (token.Substring == "@")
            {
                var operand = stack.Pop();

                var floatOperand = (operand as ConstantToken<float>)?.Value ?? identifierValues[operand as IdentifierToken].Value;
                stack.Push(new ConstantToken<float>(-floatOperand));
            }
        }
    }
}