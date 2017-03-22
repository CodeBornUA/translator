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
                Log.Error("There is not a correct number in the input stream");
            }
        }
    }
}
