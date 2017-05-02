using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Translator.LexerAnalyzer.Tokens;

namespace Parser.Executor
{
    
    public class BasicExecutor : IExecutor
    {
        public ILogger Logger { get; }

        public BasicExecutor(ILogger logger = null)
        {
            Logger = logger;
        }

        public PrnComposer PrnComposer { get; } = new PrnComposer();

        public void Execute(Context context, params string[] args)
        {
            using (var input = new MemoryStream())
            using (var output = new MemoryStream())
            {
                if (args.Any())
                {
                    var writer = new StreamWriter(input);
                    writer.Write(args.First());
                    writer.Flush();
                    input.Position = 0;
                }

                var prn = PrnComposer.GetPrn(context.TokenSequence, context.Labels, context.Variables);

                foreach (var identifier in prn.OfType<IdentifierToken>())
                {
                    context.Variables[identifier] = new ConstantToken<float>(0);
                }
                var prnExecutor = new PrnExpressionExecutor(input, output, Logger);
                prnExecutor.Output += s => Output?.Invoke(s);
                prnExecutor.ComputationStep += (s,i, stack) => ComputationStep?.Invoke(s, i, stack);
                prnExecutor.ComputeExpression(prn, context.Variables);
            }
        }

        public event Action<string> Output;
        public event Action<IList<Token>, int, Stack<Token>> ComputationStep;
    }
}
