using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser.Executor;
using Translator.LexerAnalyzer.Tokens;

namespace ParserTests
{
    [TestClass]
    public class PrnComputerTests
    {
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

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new VariableStore());

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ItComputesNegativeConstantExpression()
        {
            var expression = new Token[]
            {
                new ConstantToken<float>(2), new ConstantToken<float>(4), new StringToken("-")
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new VariableStore());

            Assert.AreEqual(-2, result);
        }

        [TestMethod]
        public void ItComputesExpressionWithUnarySubtraction()
        {
            var expression = new Token[]
            {
                new ConstantToken<float>(2), new ConstantToken<float>(4), new StringToken("@"), new StringToken("+")
            };

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new VariableStore());

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

            var result = PrnExpressionExecutor.ComputeExpression(expression,
                new VariableStore()
                {
                    [i] = new ConstantToken<float>(2)
                });

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
            PrnExpressionExecutor.ComputeExpression(expression, store);

            Assert.AreEqual(2, store[i].Value);
        }
    }
}