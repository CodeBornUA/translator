using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Translator.LexerAnalyzer
{
    public class LexerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Lexer>().AsSelf();
        }
    }
}
