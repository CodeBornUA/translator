using System;
using Autofac;
using Parser;
using Serilog.Events;
using Translator.LexerAnalyzer;
using Translator.UI.Logging;

namespace Translator.UI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule<LexerModule>();
            builder.RegisterModule<ParserServiceModule>();

            builder.RegisterType<MainWindowViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MainWindowLogObserver>().As<IObserver<LogEvent>>();
        }
    }
}