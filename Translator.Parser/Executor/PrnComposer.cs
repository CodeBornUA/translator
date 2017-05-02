using System;
using System.Collections.Generic;
using System.Linq;
using Parser.Executor.Operations;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public class PrnComposer
    {
        internal static readonly Dictionary<string, int> OperatorPriority = new Dictionary<string, int>()
        {
            ["("] = 0,
            ["["] = 0,
            ["if"] = 0,
            ["do"] = 0,
            [Environment.NewLine] = 0,
            ["writel"] = 0,
            ["readl"] = 0,
            ["to"] = 1,
            ["next"] = 1,
            [")"] = 1,
            ["]"] = 1,
            ["then"] = 1,
            ["="] = 2,
            ["or"] = 3,
            ["and"] = 4,
            ["!"] = 5,
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
            ["@"] = 8
        };

        public IList<Token> GetPrn(IList<Token> tokenSequence, IList<LabelToken> labels = null, VariableStore store = null)
        {
            var begin = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "begin"));
            var end = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "end"));

            var body = tokenSequence.Skip(begin + 1).Take(end - begin - 1).ToList();

            var prn = new List<Token>();
            var stack = new Stack<Token>();

            for (var i = 0; i < body.Count; i++)
            {
                var token = body[i];
                if (token is IdentifierToken || token is ConstantToken<float> || token is LabelToken ||
                    token.Substring == ":")
                {
                    prn.Add(token);
                    continue;
                }

                if (token.Substring == "-" && i > 0 && !(body[i - 1] is IdentifierToken) && !(body[i - 1] is ConstantToken<float>))
                {
                    stack.Push(new StringToken("@"));
                    continue;
                }

                if (stack.Any(x => x.Substring == "if") && ProcessIf(token, stack, prn, labels))
                    continue;

                if (stack.Any(x => x.Substring == "do") && ProcessFor(token, stack, prn, labels, store))
                    continue;

                if (stack.Any(x => x.Substring == "writel") && ProcessWrite(token, stack, prn, labels))
                    continue;
                if (stack.Any(x => x.Substring == "readl") && ProcessRead(token, stack, prn, labels))
                    continue;

                if (stack.Count == 0 && token.Substring != Environment.NewLine)
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
                    }

                    continue;
                }

                while (stack.Any() && OperatorPriority[stack.Peek().Substring] >= OperatorPriority[token.Substring])
                {
                    if (stack.Peek().Substring == "do")
                    {
                        break;
                    }

                    //Opening brace doesn't push anything
                    if (token.Substring == "(" || token.Substring == "[")
                    {
                        break;
                    }

                    var popped = stack.Pop();

                    //Do not write \r\n to resulting PRN
                    //TODO: do it inside of loops
                    if (popped.Substring != Environment.NewLine)
                        prn.Add(popped);
                }

                if (token.Substring != ")" && token.Substring != Environment.NewLine && token.Substring != "]")
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

            if (token.Substring == "(")
            {
                return true;
            }

            if (token.Substring == ")")
            {
                prn.Add(new WriteOperation());

                while (stack.Peek().Substring != "writel")
                {
                    prn.Add(stack.Pop());
                }

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

            if (token.Substring == "(")
            {
                return true;
            }

            if (token.Substring == ")")
            {
                prn.Add(new ReadOperation());

                while (stack.Peek().Substring != "readl")
                {
                    prn.Add(stack.Pop());
                }

                stack.Pop(); //Remove readl
                return true;
            }

            return false;
        }

        private bool ProcessFor(Token token, Stack<Token> stack, List<Token> prn, IList<LabelToken> labels, VariableStore store)
        {
            if (stack.Peek().Substring == "do" && stack.Peek().Tag == null && prn.Last() is IdentifierToken) //Identifier right after for keyword
            {
                stack.Peek().Tag = new ForContext()
                {
                    Parameter = prn.Last() as IdentifierToken
                };
            }

            var lastFor = stack.First(x => x.Substring == "do");
            var context = lastFor.Tag as ForContext;

            if (token.Substring == "to")
            {
                var workingId = new IdentifierToken($"_r{store.Count + 1}");
                store[workingId] = new ConstantToken<float>(0);
                context.ToIdentifier = workingId;

                var label = new LabelToken($"_m{labels.Count + 1}");
                labels.Add(label);
                context.LoopLabel = label;


                while (stack.Peek().Substring != "do")
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(label);
                prn.Add(new StringToken(":"));
                prn.Add(context.ToIdentifier);

                return true;
            }

            if (token.Substring == Environment.NewLine)
            {
                if (token.Line == lastFor.Line)
                {
                    var label = new LabelToken($"_m{labels.Count + 1}");
                    labels.Add(label);
                    context.ExitLabel = label;

                    while (stack.Peek().Substring != "do")
                    {
                        prn.Add(stack.Pop());
                    }

                    prn.Add(new StringToken("="));
                    prn.Add(context.Parameter);
                    prn.Add(context.ToIdentifier);
                    prn.Add(new StringToken("<="));
                    prn.Add(context.ExitLabel);
                    prn.Add(new ConditionalFalseJumpOperation());
                }

                return true;
            }

            if (token.Substring == "next")
            {
                while (stack.Peek().Substring != "do")
                {
                    prn.Add(stack.Pop());
                }

                prn.Add(context.Parameter);
                prn.Add(context.Parameter);
                prn.Add(new ConstantToken<float>(1));
                prn.Add(new StringToken("+"));
                prn.Add(new StringToken("="));

                prn.Add(context.LoopLabel);
                prn.Add(new UnconditionalJumpOperation());
                prn.Add(context.ExitLabel);
                prn.Add(new StringToken(":"));

                stack.Pop(); //Pop for

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

            if (token.Substring == "]")
            {
                //) pushes anything not farther than [
                var pop = stack.Pop();
                while (pop.Substring != "[")
                {
                    prn.Add(pop);
                    pop = stack.Pop();
                };
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