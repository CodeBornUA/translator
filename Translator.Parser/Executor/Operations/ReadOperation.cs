using System;
using System.IO;
using Serilog;
using Translator.Core;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class ReadOperation : Token, IOperation
    {
        public override TokenType Type { get; set; }
        public void Execute(ExecutorContext executorContext)
        {
            var operand = executorContext.Stack.Pop() as IdentifierToken;

            var str = new UnbufferedStreamReader(executorContext.InputStream).ReadLine();
            if (float.TryParse(str, out float res))
            {
                executorContext.Store[operand] = new ConstantToken<float>(res);
            }
            else
            {
                executorContext.Logger.Error("There is not a correct number in the input stream: {0}", str);
                throw new InvalidOperationException("Can't read a number from input stream");
            }
        }

        public override string ToString()
        {
            return "RD";
        }
    }
}
