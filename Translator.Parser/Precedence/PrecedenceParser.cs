﻿using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Precedence
{
    public partial class PrecedenceParser : IParser
    {
        private readonly PrecedenceGrammarHelper _helper;
        private readonly IObserver<LogEvent> _logObserver;
        private Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> _precedence;

        static PrecedenceParser()
        {
            InitGrammar();
        }

        public PrecedenceParser(IObserver<LogEvent> logObserver)
        {
            _logObserver = logObserver;

            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Observers(ConfigureObservers)
                .CreateLogger();

            _helper = new PrecedenceGrammarHelper(Logger);
        }

        public Logger Logger { get; set; }
        public static IList<GrammarReplaceRule> Grammar { get; } = new List<GrammarReplaceRule>();

        public Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> Precedence
        {
            get { return _precedence ?? (_precedence = _helper.GetPrecedenceTable(Grammar)); }
            set { _precedence = value; }
        }

        public bool CheckSyntax(IEnumerable<Token> tokens)
        {
            var precedenceTable = _helper.GetPrecedenceTable(Grammar);
            var tokensList = tokens as List<Token> ?? tokens.ToList();

            var sharp = TokenEnum.Sharp;
            sharp.Line = tokensList[tokensList.Count - 1].Line;
            tokensList.Add(sharp);
            var tokensArray = tokensList.ToArray();
            var array = tokensArray.Select(t =>
            {
                if (t is TokenEnum)
                    return t;
                var tEnum =
                    Grammar.SelectMany(x => x.CompositeToken)
                        .Cast<TokenEnum>()
                        .FirstOrDefault(x => x.IsTheSame(t))?.Clone() as TokenEnum;

                if (tEnum != null)
                    tEnum.Line = t.Line;

                return tEnum;
            }).ToArray();

            if (array.Any(x => x == null))
                Logger.Error("Unknown token: {0}", tokensArray[Array.FindIndex(array, x => x == null)]);

            var stack = new Stack<Token>();
            stack.Push(TokenEnum.Sharp);

            var i = 0;
            var popped = new List<Token>();
            Prn = new List<Token>();
            while (stack.Peek().Type != TokenType.Axiom || array[i] != TokenEnum.Sharp)
                try
                {
                    var relation = precedenceTable[stack.Peek()][array[i]];
                    if (relation == PrecedenceRelation.More)
                    {
                        //Base search
                        popped.Clear();
                        popped.Add(stack.Pop());
                        while (precedenceTable[stack.Peek()][popped.Last()] != PrecedenceRelation.Less)
                            popped.Add(stack.Pop());

                        popped.Reverse();
                        try
                        {
                            var toReplace = Grammar.First(x => x.CompositeToken.SequenceEqual(popped));
                            toReplace.OnReplaceAction?.Invoke(tokensArray.Skip(i-popped.Count).Take(popped.Count).ToList(), Prn);
                            PRNChanged?.Invoke(toReplace.Token, Prn);
                            stack.Push(toReplace.Token);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error("Can't replace sequence {0}, Line = {1}", popped, array[i].Line);
                            return false;
                        }
                    }
                    else
                        //Copy the symbol to stack
                    {
                        stack.Push(array[i++]);
                    }

                    OnStackChanged(stack, relation.Value,
                        new ArraySegment<Token>(tokensArray, i, tokensArray.Length - i), Prn);
                }
                catch
                {
                    Logger.Error("There is no relation for a pair {0}-{1}, Line = {2}", stack.Peek(), array[i],
                        array[i].Line);
                    return false;
                }

            return true;
        }

        public List<Token> Prn { get; set; }

        public event Action<Stack<Token>, PrecedenceRelation, ArraySegment<Token>, List<Token>> StackChanged;
        public event Action<Token, List<Token>> PRNChanged;

        private void ConfigureObservers(IObservable<LogEvent> obj)
        {
            if (_logObserver != null)
                obj.Subscribe(_logObserver);
        }

        protected virtual void OnStackChanged(Stack<Token> stack, PrecedenceRelation relation,
            ArraySegment<Token> inputTokens, List<Token> prn)
        {
            StackChanged?.Invoke(stack, relation, inputTokens, prn);
        }

        protected virtual void OnPrnChanged(Token tokenReplaced, List<Token> prn)
        {
            PRNChanged?.Invoke(tokenReplaced, prn);
        }
    }
}