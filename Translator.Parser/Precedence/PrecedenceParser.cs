using System;
using System.Collections.Generic;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Translator.Lexer;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Precedence
{
    public class PrecedenceParser : IParser
    {
        private PrecedenceGrammarHelper _helper;

        public class TokenEnum : Token
        {
            private static TokenEnum _idEnum = new TokenEnum(x => x is Identifier)
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
            private static TokenEnum _labelToken = new TokenEnum(x => x is LabelToken)
            {
                Substring = "Label"
            };
            private Predicate<Token> _equalsPredicate;


            private TokenEnum(Predicate<Token> equalsPredicate)
            {
                _equalsPredicate = equalsPredicate;
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
            public static TokenEnum LabelDef = new TokenEnum(){Substring = "<LabelDef>", Type = TokenType.Nonterminal };

            public static TokenEnum Expression = new TokenEnum(){Substring = "<Expression>", Type = TokenType.Nonterminal };
            public static TokenEnum Expression1 = new TokenEnum(){Substring = "<Expression1>", Type = TokenType.Nonterminal };
            public static TokenEnum Term = new TokenEnum(){Substring = "<Term>", Type = TokenType.Nonterminal };
            public static TokenEnum Term1 = new TokenEnum(){Substring = "<Term1>", Type = TokenType.Nonterminal };
            public static TokenEnum Mult = new TokenEnum(){Substring = "<Mult>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalExpression = new TokenEnum(){Substring = "<LogicalExpression>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalExpression1 = new TokenEnum(){Substring = "<LogicalExpression1>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalTerm = new TokenEnum(){Substring = "<LogicalTerm>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalTerm1 = new TokenEnum(){Substring = "<LogicalTerm1>", Type = TokenType.Nonterminal };
            public static TokenEnum LogicalMult = new TokenEnum(){Substring = "<LogicalMult>", Type = TokenType.Nonterminal };
            public static TokenEnum Relation = new TokenEnum(){Substring = "<Relation>", Type = TokenType.Nonterminal };
            public static TokenEnum RelationOperator = new TokenEnum(){Substring = "<RelationOperator>", Type = TokenType.Nonterminal };
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

            public override string ToString()
            {
                return Substring;
            }

            public static TokenEnum Const()
            {
                return _const;
            }
        }

        private static IList<KeyValuePair<Token, CompositeToken>> _grammar = new List<KeyValuePair<Token, CompositeToken>>();
        private IObserver<LogEvent> _logObserver;
        private Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> _precedence;

        public Logger Logger { get; set; }
        public static IList<KeyValuePair<Token, CompositeToken>> Grammar => _grammar;

        public Dictionary<Token, Dictionary<Token, PrecedenceRelation?>> Precedence
        {
            get
            {
                if (_precedence == null)
                {
                    _precedence = _helper.GetPrecedenceTable(_grammar);
                }
                return _precedence;
            }
            set { _precedence = value; }
        }

        static PrecedenceParser()
        {
            InitGrammar();
        }

        public PrecedenceParser(IObserver<LogEvent> logObserver)
        {
            _logObserver = logObserver;
            _helper = new PrecedenceGrammarHelper(Logger);

            Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Observers(ConfigureObservers)
                .CreateLogger();
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
                TokenEnum.String("program"), TokenEnum.ProgramName, TokenEnum.NewLine(),
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
                TokenEnum.String("not"), TokenEnum.LogicalMult
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Relation, new CompositeToken()
            {
                TokenEnum.Expression1, TokenEnum.RelationOperator, TokenEnum.Expression1
            }));

            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String("<") }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String("<=") }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String(">") }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String(">=") }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String("==") }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.RelationOperator, new CompositeToken(){TokenEnum.String("!=") }));
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
                TokenEnum.LabelDef, TokenEnum.UnlabeledStatement
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.LabelDef, new CompositeToken()
            {
                TokenEnum.Label(), TokenEnum.String(":")
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
                TokenEnum.String("do"), TokenEnum.Id(), TokenEnum.String("="), TokenEnum.Expression1, TokenEnum.String("to"), TokenEnum.Expression1,
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
                TokenEnum.Id()
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
                TokenEnum.Term
            }));
            _grammar.Add(new KeyValuePair<Token, CompositeToken>(TokenEnum.Expression1, new CompositeToken()
            {
                TokenEnum.Expression
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

            return true;
        }
    }
}
