using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Translator.Lexer;

namespace Parser.Precedence.Tests
{
    [TestClass()]
    public class PrecedenceGrammarHelperTests
    {
        [TestMethod()]
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
            var firstPlus = helper.FirstPlus(new List<KeyValuePair<Token, CompositeToken>>()
            {
                
            }, composite).ToList();

            Assert.AreEqual(2, firstPlus.Count);
            Assert.IsTrue(firstPlus.Any(x => x is StringToken && x.Substring == "A"));
            Assert.IsTrue(firstPlus.Any(x => x is CompositeToken && x.Substring == "Inner"));
        }
    }
}