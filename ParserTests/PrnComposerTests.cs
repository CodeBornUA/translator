using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Executor;
using Parser.Executor.Operations;
using Translator.LexerAnalyzer;
using Translator.LexerAnalyzer.Tokens;

namespace ParserTests
{
    [TestClass]
    public class PrnComposerTests
    {
        private IList<Token> GetSequence(IEnumerable<Token> body, params string[] ids)
        {
            var sequence = new List<Token>
            {
                new StringToken("program"), new IdentifierToken("test"), new StringToken(Environment.NewLine),
                new StringToken("var"), new StringToken(",") 
            };

            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                sequence.Add(new IdentifierToken(id));

                sequence.Add(i == ids.Length - 1 ? new StringToken(Environment.NewLine) : new StringToken(","));
            }

            sequence.Add(new StringToken("begin"));
            sequence.Add(new StringToken(Environment.NewLine));

            sequence.AddRange(body);

            sequence.Add(new StringToken(Environment.NewLine));
            sequence.Add(new StringToken("end"));

            return sequence;
        }

        [TestMethod]
        public void ItComposesPrnForAssignment()
        {
            var sequence = GetSequence(new Token[]
            {
                new IdentifierToken("a"), new StringToken("="), new ConstantToken<float>(2.0f)
            }, "a");

            var executor = new BasicExecutor();
            var prn = executor.GetPrn(sequence);

            Assert.AreEqual(3, prn.Count);
            Assert.AreEqual(true, prn[0] is IdentifierToken);
            Assert.AreEqual(true, prn[1] is ConstantToken<float>);
            Assert.AreEqual("=", prn[2].Substring);
        }

        [TestMethod]
        public void ItComposesPrnForAssignmentArithmeticExp()
        {
            var lexer = new Lexer();
            var sequence = lexer.ParseTokens(new StringReader(@"program test
var ,a
begin
    a = (2 + 8) * ((3 + 4) / 2)
end")).ToList();

            var executor = new BasicExecutor();
            var prn = executor.GetPrn(sequence);

            //a 2 8 + 3 4 + 2 / * =
            Assert.AreEqual(11, prn.Count);
            Assert.AreEqual(true, prn[0] is IdentifierToken);
            Assert.AreEqual(true, prn[1] is ConstantToken<float>);
            Assert.AreEqual(true, prn[2] is ConstantToken<float>);
            Assert.AreEqual("+", prn[3].Substring);
            Assert.AreEqual(true, prn[4] is ConstantToken<float>);
            Assert.AreEqual(true, prn[5] is ConstantToken<float>);
            Assert.AreEqual("+", prn[6].Substring);
            Assert.AreEqual(true, prn[7] is ConstantToken<float>);
            Assert.AreEqual("/", prn[8].Substring);
            Assert.AreEqual("*", prn[9].Substring);
            Assert.AreEqual("=", prn[10].Substring);
        }

        [TestMethod]
        public void ItComposesPrnForMultiLineAssignment()
        {
            var lexer = new Lexer();
            var sequence = lexer.ParseTokens(new StringReader(@"program test
var ,a
begin
    a = 2
    a = 3
end")).ToList();

            var executor = new BasicExecutor();
            var prn = executor.GetPrn(sequence);

            //a 2 = a 3 =
            Assert.AreEqual(6, prn.Count);
            Assert.AreEqual(true, prn[0] is IdentifierToken);
            Assert.AreEqual(true, prn[1] is ConstantToken<float>);
            Assert.AreEqual("=", prn[2].Substring);
            Assert.AreEqual(true, prn[3] is IdentifierToken);
            Assert.AreEqual(true, prn[4] is ConstantToken<float>);
            Assert.AreEqual("=", prn[5].Substring);
        }

        [TestMethod]
        public void ItComposesPrnForIfStatement()
        {
            var lexer = new Lexer();
            var sequence = lexer.ParseTokens(new StringReader(@"program test
var ,a
begin
    m: 
    if a == 1 then goto m
end")).ToList();

            var executor = new BasicExecutor();
            var labels = lexer.Labels.ToList();
            var prn = executor.GetPrn(sequence, labels);

            //m: a 1 == _m1 CondFalse m Uncond _m1:
            Assert.AreEqual(11, prn.Count);
            Assert.AreEqual(true, prn[0] is LabelToken);
            Assert.AreEqual(":", prn[1].Substring);
            Assert.AreEqual(true, prn[2] is IdentifierToken);
            Assert.AreEqual(true, prn[3] is ConstantToken<float>);
            Assert.AreEqual("==", prn[4].Substring);
            Assert.AreEqual(true, prn[5] is LabelToken);
            Assert.AreEqual(true, prn[6] is ConditionalFalseJumpOperation);
            Assert.AreEqual(true, prn[7] is LabelToken);
            Assert.AreEqual(true, prn[8] is UnconditionalJumpOperation);
            Assert.AreEqual(true, prn[9] is LabelToken);
            Assert.AreEqual(":", prn[10].Substring);

            Assert.IsTrue(labels.Any(x => x.Name == "_m2"));
        }
    }
}
