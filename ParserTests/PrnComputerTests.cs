using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Executor;
using Translator.Lexer;

namespace ParserTests
{
    [TestClass()]
    public class PrnComputerTests
    {
        [DataTestMethod]
        [DataRow(2,2,"+",4)]
        [DataRow(4,2,"-",2)]
        [DataRow(4,2,"*",8)]
        [DataRow(6,3,"/",2)]
        public void ItComputesConstantExpression(float operand1, float operand2, string operation, float expected)
        {
            var expression = new Token[]
            {
                new Constant<float>(operand1), new Constant<float>(operand2), new StringToken(operation)
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new Dictionary<Identifier, Constant<float>>());

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ItComputesNegativeConstantExpression()
        {
            var expression = new Token[]
            {
                new Constant<float>(2), new Constant<float>(4), new StringToken("-")
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new Dictionary<Identifier, Constant<float>>());

            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        public void ItComputesExpressionWithUnarySubtraction()
        {
            var expression = new Token[]
            {
                new Constant<float>(2), new Constant<float>(4), new StringToken("@") , new StringToken("+")
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new Dictionary<Identifier, Constant<float>>());

            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        public void ItComputesExpressionWithIdentifiers()
        {
            var i = new Identifier("i");
            var expression = new Token[]
            {
                i, i, new StringToken("+")
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new Dictionary<Identifier, Constant<float>>()
                {
                    [i] = new Constant<float>(2)
                });

            Assert.AreEqual(4, result);
        }
    }
}