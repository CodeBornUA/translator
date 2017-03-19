using System;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Precedence
{
    public partial class PrecedenceParser : IParser
    {
        public class TokenEnum : Token
        {
            private static readonly TokenEnum _idEnum = new TokenEnum(x => x.GetType() == typeof(IdentifierToken))
            {
                Substring = "Identifier",
            };

            private static readonly TokenEnum _newLine = new TokenEnum(x => x.Substring == "\r\n")
            {
                Substring = "New line"
            };

            private static readonly TokenEnum _const = new TokenEnum(x => x is ConstantToken<float>)
            {
                Substring = "Constant"
            };

            private static readonly TokenEnum _labelToken = new TokenEnum(x => x.GetType() == typeof(LabelToken))
            {
                Substring = "Label"
            };

            public static TokenEnum Program = new TokenEnum
            {
                Type = TokenType.Axiom,
                Substring = "<Program>"
            };

            public static TokenEnum DefList = new TokenEnum {Substring = "<DefList>", Type = TokenType.Nonterminal};
            public static TokenEnum Def = new TokenEnum {Substring = "<Def>", Type = TokenType.Nonterminal};
            public static TokenEnum DefList1 = new TokenEnum {Substring = "<DefList1>", Type = TokenType.Nonterminal};

            public static TokenEnum StatementList1 = new TokenEnum
            {
                Substring = "<StatementList1>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum StatementList = new TokenEnum
            {
                Substring = "<StatementList>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum Statement = new TokenEnum {Substring = "<Statement>", Type = TokenType.Nonterminal};

            public static TokenEnum UnlabeledStatement = new TokenEnum
            {
                Substring = "<UnlabeledStatement>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum IdList = new TokenEnum {Substring = "<IdList>", Type = TokenType.Nonterminal};
            public static TokenEnum IdList1 = new TokenEnum {Substring = "<IdList1>", Type = TokenType.Nonterminal};

            public static TokenEnum Expression = new TokenEnum
            {
                Substring = "<Expression>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum Expression1 = new TokenEnum
            {
                Substring = "<Expression1>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum Expression2 = new TokenEnum
            {
                Substring = "<Expression2>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum Term = new TokenEnum {Substring = "<Term>", Type = TokenType.Nonterminal};
            public static TokenEnum Term1 = new TokenEnum {Substring = "<Term1>", Type = TokenType.Nonterminal};
            public static TokenEnum Mult = new TokenEnum {Substring = "<Mult>", Type = TokenType.Nonterminal};

            public static TokenEnum LogicalExpression = new TokenEnum
            {
                Substring = "<LogicalExpression>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum LogicalExpression1 = new TokenEnum
            {
                Substring = "<LogicalExpression1>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum LogicalTerm = new TokenEnum
            {
                Substring = "<LogicalTerm>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum LogicalTerm1 = new TokenEnum
            {
                Substring = "<LogicalTerm1>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum LogicalMult = new TokenEnum
            {
                Substring = "<LogicalMult>",
                Type = TokenType.Nonterminal
            };

            public static TokenEnum Relation = new TokenEnum {Substring = "<Relation>", Type = TokenType.Nonterminal};

            public static TokenEnum ProgramName = new TokenEnum
            {
                Substring = "<ProgramName>",
                Type = TokenType.Nonterminal
            };


            private TokenEnum(Predicate<Token> equalsPredicate)
            {
                EqualsPredicate = equalsPredicate;
            }

            private TokenEnum()
            {
            }

            public override TokenType Type { get; set; } = TokenType.Unknown;

            public static Token Sharp { get; set; } = new TokenEnum
            {
                Substring = "#"
            };

            public Predicate<Token> EqualsPredicate { get; set; }

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
                    return (obj as StringToken).Substring == Substring;
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
                return EqualsPredicate?.Invoke(t) ?? false;
            }
        }
    }
}