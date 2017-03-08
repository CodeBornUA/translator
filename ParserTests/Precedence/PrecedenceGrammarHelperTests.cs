using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Precedence;
using Translator.Lexer;

namespace ParserTests.Precedence
{
    [TestClass()]
    public class PrecedenceGrammarHelperTests
    {
        [TestMethod]
        public void FirstPlusTest()
        {
            var innerComposite = new CompositeToken()
            {
                Substring = "Inner"
            };
            var stringToken = new StringToken();
            stringToken.Append('A');
            innerComposite.Add(stringToken);

            var composite = new CompositeToken()
            {
                innerComposite,
                new StringToken()
                {
                    Substring = "String"
                }
            };

            var helper = new PrecedenceGrammarHelper(null);
            var firstPlus = helper.FirstPlus(new List<GrammarReplaceRule>()
            {
                new GrammarReplaceRule(PrecedenceParser.TokenEnum.Program, composite)
            }, PrecedenceParser.TokenEnum.Program).ToList();

            Assert.AreEqual(2, firstPlus.Count);
            Assert.IsTrue(firstPlus.Any(x => x is StringToken && x.Substring == "A"));
            Assert.IsTrue(firstPlus.Any(x => x is CompositeToken && x.Substring == "Inner"));
        }
    }
}