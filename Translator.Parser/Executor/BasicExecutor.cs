using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parser.Executor.Operations;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public class BasicExecutor : IExecutor
    {
        private static readonly Dictionary<string, int> OperatorPriority = new Dictionary<string, int>()
        {
            ["("] = 0,
            ["if"] = 0,
            [Environment.NewLine] = 0,
            ["writel"] = 0,
            ["readl"] = 0,
            [")"] = 1,
            ["then"] = 1,
            ["="] = 2,
            ["or"] = 3,
            ["and"] = 4,
            ["not"] = 5,
            [">"] = 6,
            ["<"] = 6,
            ["<="] = 6,
            [">="] = 6,
            ["!="] = 6,
            ["=="] = 6,
            ["+"] = 7,
            ["-"] = 7,
            ["*"] = 8,
            ["/"] = 8,
        };

        public void Execute(IList<Token> tokenSequence, VariableStore variables, IList<LabelToken> labels, params string[] args)
        {
            using (var input = new MemoryStream())
            using (var output = new MemoryStream())
            {
                var prn = GetPrn(tokenSequence, labels);
                var prnExecutor = new PrnExpressionExecutor(input, output);
                prnExecutor.ComputeExpression(prn, variables);
            }
        }

        public IList<Token> GetPrn(IList<Token> tokenSequence, IList<LabelToken> labels = null)
        {
            var begin = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "begin"));
            var end = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "end"));

            var body = tokenSequence.Skip(begin + 1).Take(end - begin - 1);

            var prn = new List<Token>();
            var stack = new Stack<Token>();

            foreach (var token in body)
            {
                if (token is IdentifierToken || token is ConstantToken<float> || token is LabelToken || token.Substring == ":")
                {
                    prn.Add(token);
                    continue;
                }

                if (stack.Any(x => x.Substring == "if") && ProcessIf(token, stack, prn, labels))
                    continue;

                if (stack.Any(x => x.Substring == "writel") && ProcessWrite(token, stack, prn, labels))
                    continue;
                if (stack.Any(x => x.Substring == "readl") && ProcessRead(token, stack, prn, labels))
                    continue;

                if (stack.Count == 0)
                {
                    stack.Push(token);
                    continue;
                }

                if (token.Substring == ")")
                {
                    //) pushes anything not farther than (
                    var pop = stack.Pop();
                    while (pop.Substring != "(")
                    {
                        prn.Add(pop);
                        pop = stack.Pop();
                    };
                    continue;
                }

                while (stack.Any() && OperatorPriority[stack.Peek().Substring] >= OperatorPriority[token.Substring])
                {
                    //Opening brace doesn't push anything
                    if (token.Substring == "(")
                    {
                        break;
                    }

                    var popped = stack.Pop();

                    //Do not write \r\n to resulting PRN
                    //TODO: do it inside of loops
                    if (popped.Substring != Environment.NewLine)
                        prn.Add(popped);
                }

                if (token.Substring != ")" && token.Substring != Environment.NewLine)
                {
                    stack.Push(token);
                }
            }

            if (stack.Any())
            {
                prn.AddRange(stack);
            }

            return prn;
        }

        private bool ProcessWrite(Token token, Stack<Token> stack, List<Token> prn, IList<LabelToken> labels)
        {
            if (token.Substring == ",")
            {
                prn.Add(new WriteOperation());
                return true;
            }

            if (token.Substring == Environment.NewLine)
            {
                while (stack.Peek().Substring != "writel")
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(new WriteOperation());

                stack.Pop(); //Remove writel
                return true;
            }

            return false;
        }

        private bool ProcessRead(Token token, Stack<Token> stack, List<Token> prn, IList<LabelToken> labels)
        {
            if (token.Substring == ",")
            {
                prn.Add(new ReadOperation());
                return true;
            }

            if (token.Substring == Environment.NewLine)
            {
                while (stack.Peek().Substring != "readl")
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(new ReadOperation());

                stack.Pop(); //Remove readl
                return true;
            }

            return false;
        }

        private static bool ProcessIf(Token token, Stack<Token> stack, IList<Token> prn, IList<LabelToken> labels)
        {
            if (token.Substring == "then")
            {
                var lastIf = stack.First(x => x.Substring == "if");

                var label = new LabelToken($"_m{labels.Count + 1}");
                lastIf.Tag = label;

                labels.Add(label);

                while (stack.Peek() != lastIf)
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(label);
                prn.Add(new ConditionalFalseJumpOperation());

                return true;
            }

            if (token.Substring == "goto")
            {
                return true;
            }

            var ifToken = stack.FirstOrDefault(x => x.Substring == "if");
            if (ifToken != null && token.Substring == "\r\n")
            {
                while (stack.Peek() != ifToken)
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(new UnconditionalJumpOperation());
                prn.Add(ifToken.Tag as LabelToken);
                prn.Add(new StringToken(":"));

                stack.Pop(); //Remove if
                return true;
            }
            return false;
        }
    }
}
