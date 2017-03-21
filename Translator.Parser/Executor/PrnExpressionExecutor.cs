using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parser.Executor.Operations;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public class PrnExpressionExecutor
    {
        public Stream InputStream { get; set; }
        public Stream OutputStream { get; set; }

        public PrnExpressionExecutor(Stream input = null, Stream output = null)
        {
            InputStream = input;
            OutputStream = output;
        }

        public float? ComputeExpression(IList<Token> prn, VariableStore identifierValues)
        {
            var stack = new Stack<Token>();

            var context = new ExecutorContext(stack, identifierValues, prn)
            {
                InputStream = InputStream,
                OutputStream = OutputStream
            };
            for (var index = 0; index < prn.Count; index++)
            {
                var token = prn[index];
                var operation = token as IOperation;
                if (operation != null)
                {
                    var nextIndex = index + 1;
                    operation.Execute(context);
                    if (context.NextPosition != null)
                    {
                        nextIndex = context.NextPosition.Value;
                    }

                    index = nextIndex - 1;
                    continue;
                }

                if (token.Substring == ":")
                {
                    continue;
                }

                ProcessIndentifiersConstants(token, stack);
                ProcessUnarySubtraction(identifierValues, token, stack);
                ProcessBoolean(identifierValues, token, stack);
                ProcessArithmeticOperations(identifierValues, token, stack);
                ProcessAssignment(identifierValues, token, stack);
            }

            if (stack.Any())
            {
                var popped = stack.Pop();
                if (popped is ConstantToken<float> || popped is IdentifierToken)
                {
                    return (popped as ConstantToken<float>)?.Value ?? identifierValues[popped as IdentifierToken].Value;
                }
            }

            return null;
        }

        private static void ProcessAssignment(VariableStore identifierValues, Token token, Stack<Token> stack)
        {
            if (token.Substring == "=")
            {
                var operand2 = stack.Pop();
                var operand1 = stack.Pop();

                identifierValues[operand1 as IdentifierToken] = operand2 as ConstantToken<float>;
            }
        }

        private static void ProcessArithmeticOperations(VariableStore identifierValues,
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

        private static void ProcessBoolean(VariableStore identifierValues, Token token, Stack<Token> stack)
        {
            if (token.Substring == ">" || token.Substring == "<" || token.Substring == "=="
                || token.Substring == "<=" || token.Substring == ">=" || token.Substring == "!=")
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
                    case ">":
                        localResult = floatOperand1 > floatOperand2 ? 1.0f : 0.0f;
                        break;
                    case "<":
                        localResult = floatOperand1 < floatOperand2 ? 1.0f : 0.0f;
                        break;
                    case "<=":
                        localResult = floatOperand1 <= floatOperand2 ? 1.0f : 0.0f;
                        break;
                    case ">=":
                        localResult = floatOperand1 >= floatOperand2 ? 1.0f : 0.0f;
                        break;
                    case "==":
                        localResult = floatOperand1 == floatOperand2 ? 1.0f : 0.0f;
                        break;
                    case "!=":
                        localResult = floatOperand1 != floatOperand2 ? 1.0f : 0.0f;
                        break;
                }
                stack.Push(new ConstantToken<float>(localResult));
            }
        }

        private static void ProcessIndentifiersConstants(Token token, Stack<Token> stack)
        {
            if (token is IdentifierToken || token is ConstantToken<float> || token is LabelToken)
                stack.Push(token);
        }

        private static void ProcessUnarySubtraction(VariableStore identifierValues,
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