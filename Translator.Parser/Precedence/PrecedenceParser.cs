using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Translator.Lexer;
using Translator.LexerAnalyzer.Tokens;
using System.Linq;

namespace Parser.Precedence
{
    public class PrecedenceParser : IParser
    {
        private PrecedenceGrammarHelper _helper;

        public class TokenEnum : Token
        {
            private static TokenEnum _idEnum = new TokenEnum(x => x.GetType() == typeof(Identifier))
            {
                Substring = "Identifier"
            };
            private static TokenEnum _newLine = new TokenEnum(x => x.Substring == "\r\n")
            {
                Substring = "New line"
            };
            private static TokenEnum _const = new TokenEnum(x => x is Constant<float>)
            {
                Substring = "Constant"
            };
            private static TokenEnum _labelToken = new TokenEnum(x => x.GetType() == typeof(LabelToken))
            {
                Substring = "Label"
            };


            private TokenEnum(Predicate<Token> equalsPredicate)
            {
                EqualsPredicate = equalsPredicate;
            }

            public static TokenEnum Program = new TokenEnum()
            {
                Type = TokenType.Axiom,
                Substring = "<Program>"
            };

            public static TokenEnum DefList = new TokenEnum(){Substring = "<DefList>", Type = TokenType.Nonterminal};
            public static TokenEnum Def = new TokenEnum(){Substring = "<Def>", Type = TokenType.Nonterminal };
            public static TokenEnum DefList1 = new TokenEnum(){Substring = "<DefList1>", Type = TokenType.Nonterminal };

            public static TokenEnum StatementList1 = new TokenEnum(){Substring = "<StatementList1>", Type = TokenType.Nonterminal };
            public static TokenEnum StatementList = new TokenEnum(){Substring = "<StatementList>", Type = TokenType.Nonterminal };
            public static TokenEnum Statement = new TokenEnum(){Substring = "<Statement>", Type = TokenType.Nonterminal };

            public static TokenEnum UnlabeledStatement = new TokenEnum(){Substring = "<UnlabeledStatement>", Type = TokenType.Nonterminal };
            public static TokenEnum IdList = new TokenEnum(){Substring = "<IdList>", Type = TokenType.Nonterminal };
            public static TokenEnum IdList1 = new TokenEnum(){Substring = "<IdList1>", Type = TokenType.Nonterminal };

            public static TokenEnum Expression = new TokenEnum(){Substring = "<Expression>", Type = TokenType.Nonterminal };
            public static TokenEnum Expression1 = new TokenEnum(){Substring = "<Expression1>", Type = TokenType.Nonterminal };
            public static TokenEnum Expression2 = new TokenEnum(){Substring = "<Expression2>", Type = TokenType.Nonterminal };
            public static TokenEnum Term = new TokenEnum(){Substring = "<Term>", Type = TokenType.Nonterminal };
            public static TokenEnum Term1 = new TokenEnum(){Substring = "<Term1>", Type = TokenType.Nonterminal };
            public static TokenEnum Mult = new TokenEnum(){Substring = "<Mult>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalExpression = new TokenEnum(){Substring = "<LogicalExpression>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalExpression1 = new TokenEnum(){Substring = "<LogicalExpression1>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalTerm = new TokenEnum(){Substring = "<LogicalTerm>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalTerm1 = new TokenEnum(){Substring = "<LogicalTerm1>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalMult = new TokenEnum(){Substring = "<LogicalMult>", Type = TokenType.Nonterminal };
            public static TokenEnum Relation = new TokenEnum(){Substring = "<Relation>", Type = TokenType.Nonterminal };
            public static TokenEnum ProgramName = new TokenEnum(){Substring = "<ProgramName>", Type = TokenType.Nonterminal };

            private TokenEnum()
            {
                
            }

            public static TokenEnum String(string value)
            {
                return new TokenEnum(t => t.Substring == value)
                {
                    Substring = value
                };
            }

            public override bool Equals(object obj)
            {
                if (obj is StringToken)
                {
                    return (obj as StringToken).Substring == Substring;
                }
                return ReferenceEquals(this, obj);
            }

            public static TokenEnum Id()
            {
                return _idEnum;
            }

            public static TokenEnum NewLine()
            {
                return _newLine;
            }

            public static TokenEnum Label()
            {
                return _labelToken;
            }

            public override TokenType Type { get; set; } = TokenType.Unknown;

            public static Token Sharp { get; set; } = new TokenEnum()
            {
                Substring = "#"
            };

            public Predicate<Token> EqualsPredicate { get; set; }

            public override string ToString()
            {
                return Substring;
            }

            public static TokenEnum Const()
            {
                return _const;
            }

            internal bool IsTheSame(Token t)
            {
                return EqualsPredicate?.Invoke(t as Token) ?? false;
            }
        }

        private static IList<KeyValuePair<Token, CompositeToken>> _grammar = new List<KeyValuePair<Token, CompositeToken>>();
        private IObserver<LogEvent> _logObserver;
        private Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> _precedence;

        public Logger Logger { get; set; }
        public static IList<KeyValuePair<Token, CompositeToken>> Grammar => _grammar;

        public event Action<Stack<Token>, PrecedenceRelation, ArraySegment<Token>> StackChanged; 

        public Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> Precedence
        {
            get { return _precedence ?? (_precedence = _helper.GetPrecedenceTable(_grammar)); }
            set { _precedence = value; }
        }

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

        private void ConfigureObservers(IObservable<LogEvent> obj)
        {
            if (_logObserver != null)
            {
                obj.Subscribe(_logObserver);
            }
        }

        public static void InitGrammar()
        {
            //Program rule
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Program, new CompositeToken()
            {
                TokenEnum.ProgramName, TokenEnum.NewLine(),
                TokenEnum.String("var"), TokenEnum.DefList1, TokenEnum.NewLine(),
                TokenEnum.String("begin"),
                TokenEnum.StatementList1,
                TokenEnum.String("end")
            }));

            //Definition list
            DefinitionList();

            //Statement list
            StatementList();

            //Unlabeled operator
            UnlabeledOperator();

            IdList();

            //Expression
            FillExpression();

            FillLogicalExpression();
        }

        private static void FillLogicalExpression()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalExpression1, new CompositeToken()
            {
                TokenEnum.LogicalExpression
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalExpression, new CompositeToken()
            {
                TokenEnum.LogicalTerm1
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalExpression, new CompositeToken()
            {
                TokenEnum.LogicalExpression, TokenEnum.String("or"), TokenEnum.LogicalTerm1
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalTerm, new CompositeToken()
            {
                TokenEnum.LogicalMult
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalTerm1, new CompositeToken()
            {
                TokenEnum.LogicalTerm
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalTerm, new CompositeToken()
            {
                TokenEnum.LogicalTerm, TokenEnum.String("and"), TokenEnum.LogicalMult 
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalMult, new CompositeToken()
            {
                TokenEnum.Relation
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalMult, new CompositeToken()
            {
                TokenEnum.String("["), TokenEnum.LogicalExpression1, TokenEnum.String("]")
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LogicalMult, new CompositeToken()
            {
                TokenEnum.String("!"), TokenEnum.LogicalMult
            }));

            var relationsOps = new[]
            {
                TokenEnum.String("<"),
                TokenEnum.String("<="),
                TokenEnum.String(">"),
                TokenEnum.String(">="),
                TokenEnum.String("=="),
                TokenEnum.String("!="),     
            };
            foreach (var relationsOp in relationsOps)
            {
                _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Relation, new CompositeToken()
                {
                    TokenEnum.Expression1,
                    relationsOp,
                    TokenEnum.Expression1
                }));
            }  
        }

        private static void DefinitionList()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.DefList, new CompositeToken()
            {
                TokenEnum.DefList, TokenEnum.String(";"), TokenEnum.Def
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.DefList, new CompositeToken()
            {
                TokenEnum.Def
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.DefList1, new CompositeToken()
            {
                TokenEnum.DefList
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Def, new CompositeToken()
            {
                TokenEnum.IdList1, TokenEnum.String(":"), TokenEnum.String("float")
            }));
        }

        private static void StatementList()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.StatementList, new CompositeToken()
            {
                TokenEnum.StatementList, TokenEnum.NewLine(), TokenEnum.Statement
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.StatementList, new CompositeToken()
            {
                TokenEnum.NewLine(), TokenEnum.Statement
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Statement, new CompositeToken()
            {
                TokenEnum.UnlabeledStatement
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Statement, new CompositeToken()
            {
                TokenEnum.Label(), TokenEnum.String(":"), TokenEnum.UnlabeledStatement
            }));


            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.StatementList1, new CompositeToken()
            {
                TokenEnum.StatementList, TokenEnum.NewLine()
            }));
        }

        private static void UnlabeledOperator()
        {
            Assignment();
            InputOutput();
            Loop();
            If();
        }

        private static void Loop()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.UnlabeledStatement, new CompositeToken()
            {
                TokenEnum.String("do"), TokenEnum.Id(), TokenEnum.String("="), TokenEnum.Expression1, TokenEnum.String("to"), TokenEnum.Expression2,
                TokenEnum.StatementList1,
                TokenEnum.String("next")
            }));
        }

        private static void If()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.UnlabeledStatement, new CompositeToken()
            {
                TokenEnum.String("if"), TokenEnum.LogicalExpression1, TokenEnum.String("then"), TokenEnum.String("goto"), TokenEnum.Label()
            }));
        }

        private static void IdList()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.IdList1, new CompositeToken()
            {
                TokenEnum.IdList
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.IdList, new CompositeToken()
            {
                TokenEnum.String(","), TokenEnum.Id()
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.IdList, new CompositeToken()
            {
                TokenEnum.IdList, TokenEnum.String(","), TokenEnum.Id()
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.ProgramName, new CompositeToken()
            {
                TokenEnum.String("program"), TokenEnum.Id()
            }));
        }

        private static void Assignment()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.UnlabeledStatement, new CompositeToken()
            {
                TokenEnum.Id(), TokenEnum.String("="), TokenEnum.Expression1
            }));
        }

        private static void InputOutput()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.UnlabeledStatement, new CompositeToken()
            {
                TokenEnum.String("readl"), TokenEnum.String("("), TokenEnum.IdList1, TokenEnum.String(")")
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.UnlabeledStatement, new CompositeToken()
            {
                TokenEnum.String("writel"), TokenEnum.String("("), TokenEnum.IdList1, TokenEnum.String(")")
            }));
        }

        private static void FillExpression()
        {
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression, new CompositeToken()
            {
                TokenEnum.Term1
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression1, new CompositeToken()
            {
                TokenEnum.Expression
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression2, new CompositeToken()
            {
                TokenEnum.Expression1
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression, new CompositeToken()
            {
                TokenEnum.Expression, TokenEnum.String("+"), TokenEnum.Term1
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression, new CompositeToken()
            {
                TokenEnum.Expression, TokenEnum.String("-"), TokenEnum.Term1
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression, new CompositeToken()
            {
                TokenEnum.String("-"), TokenEnum.Term1
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Term1, new CompositeToken()
            {
                TokenEnum.Term
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Term, new CompositeToken()
            {
                TokenEnum.Mult
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Term, new CompositeToken()
            {
                TokenEnum.Term, TokenEnum.String("*"), TokenEnum.Mult
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Term, new CompositeToken()
            {
                TokenEnum.Term, TokenEnum.String("/"), TokenEnum.Mult
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Mult, new CompositeToken()
            {
                TokenEnum.String("("), TokenEnum.Expression1, TokenEnum.String(")")
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Mult, new CompositeToken()
            {
                TokenEnum.Id()
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Mult, new CompositeToken()
            {
                TokenEnum.Const()
            }));
        }

        public bool CheckSyntax(IEnumerable<Token> tokens)
        {
            var precedenceTable = _helper.GetPrecedenceTable(_grammar);
            var tokensList = tokens as List<Token> ?? tokens.ToList();

            var sharp = TokenEnum.Sharp;
            sharp.Line = tokensList[tokensList.Count - 1].Line;
            tokensList.Add(sharp);
            var tokensArray = tokensList.ToArray();
            var array = tokensArray.Select(t =>
            {
                if (t is TokenEnum)
                {
                    return t;
                }
                var tEnum = _grammar.SelectMany(x => x.Value).Cast<TokenEnum>().FirstOrDefault(x => x.IsTheSame(t))?.Clone() as TokenEnum;

                if (tEnum != null)
                {
                    tEnum.Line = t.Line;
                }

                return tEnum;
            }).ToArray();

            if (array.Any(x => x == null))
            {
                Logger.Error("Unknown token: {0}", tokensArray[Array.FindIndex(array, x => x == null)]);
            }

            var stack = new Stack<Token>();
            stack.Push(TokenEnum.Sharp);

            var i = 0;
            var popped = new List<Token>();
            while (stack.Peek().Type != TokenType.Axiom || array[i] != TokenEnum.Sharp)
            {
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
                            var toReplace = _grammar.First(x => x.Value.SequenceEqual(popped));
                            stack.Push(toReplace.Key);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error("Can't replace sequence {0}, Line = {1}", popped, array[i].Line);
                            return false;
                        }
                    }
                    else
                        //Copy the symbol to stack
                        stack.Push(array[i++]);

                    OnStackChanged(stack, relation.Value, new ArraySegment<Token>(tokensArray, i, tokensArray.Length - i));
                }
                catch
                {
                    Logger.Error("There is no relation for a pair {0}-{1}, Line = {2}", stack.Peek(), array[i], array[i].Line);
                    return false;
                }
            }

            return true;
        }

        protected virtual void OnStackChanged(Stack<Token> stack, PrecedenceRelation relation, ArraySegment<Token> inputTokens)
        {
            StackChanged?.Invoke(stack, relation, inputTokens);
        }
    }
}
