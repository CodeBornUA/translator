﻿using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Executor;
using Parser.Executor.Operations;
using Serilog;
using Translator.LexerAnalyzer;
using Translator.LexerAnalyzer.Tokens;

namespace ParserTests
{
    [TestClass]
    public class PrnComputerTests
    {
        private string IfTestProgram = @"
program test
var ,a,b,c,res : float
begin
    t: a = 1
    if a == 1 then goto test2
    b = 2
    test2: c = 3
    res = (a+3)*2+c
end";

        [DataTestMethod]
        [DataRow(2, 2, "+", 4)]
        [DataRow(4, 2, "-", 2)]
        [DataRow(4, 2, "*", 8)]
        [DataRow(6, 3, "/", 2)]
        public void ItComputesConstantExpression(float operand1, float operand2, string operation, float expected)
        {
            var expression = new Token[]
            {
                new ConstantToken<float>(operand1), new ConstantToken<float>(operand2), new StringToken(operation)
            };

            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, new VariableStore());

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ItComputesNegativeConstantExpression()
        {
            var expression = new Token[]
            {
                new ConstantToken<float>(2), new ConstantToken<float>(4), new StringToken("-")
            };

            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, new VariableStore());

            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        public void ItComputesExpressionWithUnarySubtraction()
        {
            var expression = new Token[]
            {
                new ConstantToken<float>(2), new ConstantToken<float>(4), new StringToken("@"), new StringToken("+")
            };

            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, new VariableStore());

            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        public void ItComputesExpressionWithIdentifiers()
        {
            var i = new IdentifierToken("i");
            var expression = new Token[]
            {
                i, i, new StringToken("+")
            };

            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, new VariableStore()
            {
                [i] = new ConstantToken<float>(2)
            });
            ;

            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void ItMakesAssignments()
        {
            var i = new IdentifierToken("i");
            var expression = new Token[]
            {
                i, new ConstantToken<float>(2), new StringToken("=")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(0)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(2, store[i].Value);
        }

        [DataTestMethod]
        [DataRow(2, true)]
        [DataRow(3, false)]
        public void ItComputesLogicalExpressions(int iValue, bool success)
        {
            var i = new IdentifierToken("i");
            var res = new IdentifierToken("res");
            var expression = new Token[]
            {
                res, i, new ConstantToken<float>(2), new StringToken("=="), new StringToken("=")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(iValue),
                [res] = new ConstantToken<float>(0)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(success ? 1 : 0, store[res].Value);
        }

        [TestMethod]
        public void ItComputesComplexExpressions()
        {
            var i = new IdentifierToken("i");
            var res = new IdentifierToken("res");
            var expression = new Token[]
            {
                res, new StringToken("["), i, new ConstantToken<float>(2), new StringToken("=="), i, new ConstantToken<float>(3), new StringToken("<"), new StringToken("and"),
                new StringToken("]"), new StringToken("=")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(2),
                [res] = new ConstantToken<float>(0)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(1, store[res].Value);
        }

        [TestMethod]
        public void ItComputesNegatedLogicalExpressions()
        {
            var i = new IdentifierToken("i");
            var res = new IdentifierToken("res");
            var expression = new Token[]
            {
                res, i, new ConstantToken<float>(2), new StringToken("=="), new StringToken("!"), new StringToken("=")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(2),
                [res] = new ConstantToken<float>(1)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(0, store[res].Value);
        }

        [TestMethod]
        public void ItMakesConditionalJumps()
        {
            var i = new IdentifierToken("i");
            var res = new IdentifierToken("res");
            var expression = new Token[]
            {
                i, new ConstantToken<float>(1), new StringToken("=="), new LabelToken("t"), new ConditionalFalseJumpOperation(),
                res, new ConstantToken<float>(1), new StringToken("="),
                new LabelToken("t"), new StringToken(":")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(0),
                [res] = new ConstantToken<float>(0)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(0, store[res].Value);
        }

        [TestMethod]
        public void ItMakesUnconditionalJumps()
        {
            var i = new IdentifierToken("i");
            var expression = new Token[]
            {
                new LabelToken("t"), new StringToken(":"),
                new LabelToken("test"), new UnconditionalJumpOperation(),
                i, new ConstantToken<float>(1), new StringToken("="),
                new LabelToken("test"), new StringToken(":")
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(0)
            };
            var executor = new PrnExpressionExecutor();
            var result = executor.ComputeExpression(expression, store);

            Assert.AreEqual(0, store[i].Value);
        }

        [TestMethod]
        public void ItWritesDataToOutput()
        {
            //Arrange
            var i = new IdentifierToken("i");
            var expression = new Token[]
            {
                i, new WriteOperation()
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(5)
            };
            using (var memoryStream = new MemoryStream())
            {
                var executor = new PrnExpressionExecutor(null, memoryStream);

                //Act
                var result = executor.ComputeExpression(expression, store);

                //Assert
                memoryStream.Position = 0;
                Assert.AreEqual("i = 5", new StreamReader(memoryStream).ReadToEnd());
            }
        }

        [TestMethod]
        public void ItReadsDataFromInput()
        {
            //Arrange
            var i = new IdentifierToken("i");
            var expression = new Token[]
            {
                i, new ReadOperation()
            };

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(0)
            };
            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream);
                writer.WriteLine("5");
                writer.Flush();
                memoryStream.Position = 0;

                var executor = new PrnExpressionExecutor(memoryStream);

                //Act
                var result = executor.ComputeExpression(expression, store);

                //Assert
                memoryStream.Position = 0;
                Assert.AreEqual(5, store[i].Value);
            }
        }

        [TestMethod]
        public void ItExecutesLoop()
        {
            var logger = new LoggerConfiguration().CreateLogger();

            //Arrange
            var i = new IdentifierToken("i");
            var j = new IdentifierToken("j");
            var res = new IdentifierToken("sum");
            var lexer = new Lexer(logger);
            var sequence = lexer.ParseTokens(new StringReader(@"
begin
    do i=1 to 5
        do j=2 to 7
            sum = sum + i*j
        next
    next
end")).ToList();

            var store = new VariableStore()
            {
                [i] = new ConstantToken<float>(0),
                [j] = new ConstantToken<float>(0),
                [res] = new ConstantToken<float>(0)
            };

            var prnProvider = new BasicExecutor(logger);
            var labels = lexer.Labels.ToList();
            var prn = prnProvider.PrnComposer.GetPrn(sequence, labels, store);


            var executor = new PrnExpressionExecutor();

            //Act
            var result = executor.ComputeExpression(prn, store);

            //Assert
            Assert.AreEqual(5 + 1, store[i].Value);
            Assert.AreEqual(7 + 1, store[j].Value);
            Assert.AreEqual(405, store[res].Value);

        }
    }
}