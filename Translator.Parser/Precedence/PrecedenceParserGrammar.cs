using System.Linq;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Precedence
{
    public partial class PrecedenceParser
    {
        public static void InitGrammar()
        {
            //Program rule
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Program, new CompositeToken
            {
                TokenEnum.ProgramName,
                TokenEnum.NewLine(),
                TokenEnum.String("var"),
                TokenEnum.DefList1,
                TokenEnum.NewLine(),
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
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalExpression1, new CompositeToken
            {
                TokenEnum.LogicalExpression
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalExpression, new CompositeToken
            {
                TokenEnum.LogicalTerm1
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalExpression, new CompositeToken
            {
                TokenEnum.LogicalExpression,
                TokenEnum.String("or"),
                TokenEnum.LogicalTerm1
            }));

            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalTerm, new CompositeToken
            {
                TokenEnum.LogicalMult
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalTerm1, new CompositeToken
            {
                TokenEnum.LogicalTerm
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalTerm, new CompositeToken
            {
                TokenEnum.LogicalTerm,
                TokenEnum.String("and"),
                TokenEnum.LogicalMult
            }));

            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalMult, new CompositeToken
            {
                TokenEnum.Relation
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalMult, new CompositeToken
            {
                TokenEnum.String("["),
                TokenEnum.LogicalExpression1,
                TokenEnum.String("]")
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.LogicalMult, new CompositeToken
            {
                TokenEnum.String("!"),
                TokenEnum.LogicalMult
            }));

            var relationsOps = new[]
            {
                TokenEnum.String("<"),
                TokenEnum.String("<="),
                TokenEnum.String(">"),
                TokenEnum.String(">="),
                TokenEnum.String("=="),
                TokenEnum.String("!=")
            };
            foreach (var relationsOp in relationsOps)
                Grammar.Add(new GrammarReplaceRule(TokenEnum.Relation, new CompositeToken
                {
                    TokenEnum.Expression1,
                    relationsOp,
                    TokenEnum.Expression1
                }));
        }

        private static void DefinitionList()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.DefList, new CompositeToken
            {
                TokenEnum.DefList,
                TokenEnum.String(";"),
                TokenEnum.Def
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.DefList, new CompositeToken
            {
                TokenEnum.Def
            }));

            Grammar.Add(new GrammarReplaceRule(TokenEnum.DefList1, new CompositeToken
            {
                TokenEnum.DefList
            }));

            Grammar.Add(new GrammarReplaceRule(TokenEnum.Def, new CompositeToken
            {
                TokenEnum.IdList1,
                TokenEnum.String(":"),
                TokenEnum.String("float")
            }));
        }

        private static void StatementList()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.StatementList, new CompositeToken
            {
                TokenEnum.StatementList,
                TokenEnum.NewLine(),
                TokenEnum.Statement
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.StatementList, new CompositeToken
            {
                TokenEnum.NewLine(),
                TokenEnum.Statement
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Statement, new CompositeToken
            {
                TokenEnum.UnlabeledStatement
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Statement, new CompositeToken
            {
                TokenEnum.Label(),
                TokenEnum.String(":"),
                TokenEnum.UnlabeledStatement
            }));


            Grammar.Add(new GrammarReplaceRule(TokenEnum.StatementList1, new CompositeToken
            {
                TokenEnum.StatementList,
                TokenEnum.NewLine()
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
            Grammar.Add(new GrammarReplaceRule(TokenEnum.UnlabeledStatement, new CompositeToken
            {
                TokenEnum.String("do"),
                TokenEnum.Id(),
                TokenEnum.String("="),
                TokenEnum.Expression1,
                TokenEnum.String("to"),
                TokenEnum.Expression2,
                TokenEnum.StatementList1,
                TokenEnum.String("next")
            }));
        }

        private static void If()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.UnlabeledStatement, new CompositeToken
            {
                TokenEnum.String("if"),
                TokenEnum.LogicalExpression1,
                TokenEnum.String("then"),
                TokenEnum.String("goto"),
                TokenEnum.Label()
            }));
        }

        private static void IdList()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.IdList1, new CompositeToken
            {
                TokenEnum.IdList
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.IdList, new CompositeToken
            {
                TokenEnum.String(","),
                TokenEnum.Id()
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.IdList, new CompositeToken
            {
                TokenEnum.IdList,
                TokenEnum.String(","),
                TokenEnum.Id()
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.ProgramName, new CompositeToken
            {
                TokenEnum.String("program"),
                TokenEnum.Id()
            }));
        }

        private static void Assignment()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.UnlabeledStatement, new CompositeToken
            {
                TokenEnum.Id(),
                TokenEnum.String("="),
                TokenEnum.Expression1
            }));
        }

        private static void InputOutput()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.UnlabeledStatement, new CompositeToken
            {
                TokenEnum.String("readl"),
                TokenEnum.String("("),
                TokenEnum.IdList1,
                TokenEnum.String(")")
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.UnlabeledStatement, new CompositeToken
            {
                TokenEnum.String("writel"),
                TokenEnum.String("("),
                TokenEnum.IdList1,
                TokenEnum.String(")")
            }));
        }

        private static void FillExpression()
        {
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression, new CompositeToken
            {
                TokenEnum.Term1
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression1, new CompositeToken
            {
                TokenEnum.Expression
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression2, new CompositeToken
            {
                TokenEnum.Expression1
            }));

            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression, new CompositeToken
            {
                TokenEnum.Expression,
                TokenEnum.String("+"),
                TokenEnum.Term1
            })
            {
                OnReplaceAction = (stack, list) => list.Add(new StringToken("+"))
            });
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression, new CompositeToken
            {
                TokenEnum.Expression,
                TokenEnum.String("-"),
                TokenEnum.Term1
            })
            {
                OnReplaceAction = (stack, list) => list.Add(new StringToken("-"))
            });
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Expression, new CompositeToken
            {
                TokenEnum.String("-"),
                TokenEnum.Term1
            })
            {
                OnReplaceAction = (stack, list) => list.Add(new StringToken("@"))
            });

            Grammar.Add(new GrammarReplaceRule(TokenEnum.Term1, new CompositeToken
            {
                TokenEnum.Term
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Term, new CompositeToken
            {
                TokenEnum.Mult
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Term, new CompositeToken
            {
                TokenEnum.Term,
                TokenEnum.String("*"),
                TokenEnum.Mult
            })
            {
                OnReplaceAction = (stack, list) => list.Add(new StringToken("*"))
            });
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Term, new CompositeToken
            {
                TokenEnum.Term,
                TokenEnum.String("/"),
                TokenEnum.Mult
            })
            {
                OnReplaceAction = (stack, list) => list.Add(new StringToken("/"))
            });

            Grammar.Add(new GrammarReplaceRule(TokenEnum.Mult, new CompositeToken
            {
                TokenEnum.String("("),
                TokenEnum.Expression1,
                TokenEnum.String(")")
            }));
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Mult, new CompositeToken
            {
                TokenEnum.Id()
            })
            {
                OnReplaceAction = (popped, list) => list.Add(popped.Last())
            });
            Grammar.Add(new GrammarReplaceRule(TokenEnum.Mult, new CompositeToken
            {
                TokenEnum.Const()
            })
            {
                OnReplaceAction = (popped, list) => list.Add(popped.Last())
            });
        }
    }
}