using System;
using System.Collections.Generic;
using System.Linq;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    public class BasicExecutor : IExecutor
    {
        private static readonly Dictionary<string, int> OperatorPriority = new Dictionary<string, int>()
        {
            ["("] = 0,
            [Environment.NewLine] = 0,
            [")"] = 1,
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

        public void Execute(IList<Token> tokenSequence, params string[] args)
        {
            var prn = GetPrn(tokenSequence);
        }

        public IList<Token> GetPrn(IList<Token> tokenSequence)
        {
            var begin = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "begin"));
            var end = tokenSequence.IndexOf(tokenSequence.First(x => x.Substring == "end"));

            var body = tokenSequence.Skip(begin + 1).Take(end - begin - 1);

            var prn = new List<Token>();
            var stack = new Stack<Token>();

            foreach (var token in body)
            {
                if (token is IdentifierToken || token is ConstantToken<float>)
                {
                    prn.Add(token);
                    continue;
                }

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
    }
}
