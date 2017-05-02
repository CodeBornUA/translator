using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Translator.LexerAnalyzer;
using Translator.LexerAnalyzer.Tokens;

namespace Translator.LexerAnalyzerTests
{
    [TestClass]
    public class LexerTests
    {
        public string TestPrecedenceProgram = @"program test
var ,a,b,c,res : float
begin
    a = 1
    b = 2
    c = 3
    res = (a+b)*2+c
end";
        public string TestProgram = @"program test
var float a, float b, float c
begin
    lbl:
    a = a - 1
    if a>0 then goto lbl
    do c = 1 to 10
        writel(c)
    next
end
";

        [TestMethod]
        public void LexerTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("\nprogram\n\n"));

            Assert.IsNotNull(tokens);
            Assert.IsTrue(tokens.Count == 3);
            Assert.AreEqual("\n", tokens.ElementAt(0).ToString());
            Assert.AreEqual("program", tokens.ElementAt(1).ToString());
            Assert.AreEqual("\n\n", tokens.ElementAt(2).ToString());
        }

        [DataTestMethod]
        [DataRow("a")]
        [DataRow("a234")]
        public void IdentifierTest(string name)
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader(name));

            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(new IdentifierToken(name), tokens.ElementAt(0));
        }

        [TestMethod]
        public void IdentifierCanNotStartWithNumberTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("125abc"));

            Assert.IsNotNull(tokens);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(new ConstantToken<float>("125"), tokens.ElementAt(0));
            Assert.AreEqual(new IdentifierToken("abc"), tokens.ElementAt(1));
        }

        [DataTestMethod]
        [DataRow(".21")]
        [DataRow("15.21")]
        [DataRow(".34")]
        [DataRow("48")]
        public void ConstTest(string constText)
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader(constText));

            Assert.IsNotNull(tokens);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(new ConstantToken<float>(constText), tokens.ElementAt(0));
        }

        [TestMethod]
        public void MultipleConstTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("152.28.46"));

            Assert.IsNotNull(tokens);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(new ConstantToken<float>("152.28"), tokens.ElementAt(0));
            Assert.AreEqual(new ConstantToken<float>(".46"), tokens.ElementAt(1));
        }

        [TestMethod]
        public void MultipleTokensTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("program test"));

            Assert.IsNotNull(tokens);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual("program", tokens.ElementAt(0).ToString());
            Assert.AreEqual(new IdentifierToken("test"), tokens.ElementAt(1));
        }

        [TestMethod]
        public void RightLineNumberTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("program test\n" +
                                                            "var float a, float b, float c"));

            Assert.IsNotNull(tokens);
            Assert.IsTrue(tokens.TakeWhile(x => x.Line == 1).Count() == 3);
            Assert.IsTrue(tokens.Skip(3).All(x => x.Line == 2));
        }

        [TestMethod]
        public void RightIndexes()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("program test\n" +
                                                            "var float a, float b, float c"));

            Assert.IsNotNull(tokens);
            var ids = tokens.OfType<IdentifierToken>().Select(x => x.Index.Value).ToArray();
            var expected = Enumerable.Range(0, ids.Length).ToArray();
            Assert.IsTrue(expected.SequenceEqual(ids));
        }

        [TestMethod]
        public void IdsRepeatingTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader("float a, float b, float a"));

            Assert.IsNotNull(tokens);
            var ids = tokens.OfType<IdentifierToken>().ToArray();
            Assert.AreEqual(ids.First().Index, ids.Last().Index);
        }

        [TestMethod]
        public void FullProgramTest()
        {
            var lexer = new Lexer();
            var tokens = lexer.ParseTokens(new StringReader(TestProgram));

            Assert.IsNotNull(tokens);
        }
    }
}