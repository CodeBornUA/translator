using System.Text;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor.Operations
{
    public class WriteOperation : Token, IOperation
    {
        public override TokenType Type { get; set; }
        public void Execute(ExecutorContext executorContext)
        {
            var operand = executorContext.Stack.Pop() as IdentifierToken;

            var text = $"{operand.Name} = {executorContext.Store[operand].Value}";
            var encoded = Encoding.Default.GetBytes(text);
            executorContext.OutputStream.Write(encoded, 0, encoded.Length);
        }

        public override string ToString()
        {
            return "WR";
        }
    }
}
