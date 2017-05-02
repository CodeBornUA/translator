using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Logging;
using Parser;
using Serilog;
using Serilog.Events;
using Translator.LexerAnalyzer;
using Translator.UI.Logging;
using ILogger = Serilog.ILogger;

namespace Translator.UI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<MainWindowViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<MainWindowLogObserver>().As<IObserver<LogEvent>>();

            builder.RegisterInstance(new LoggerFactory().AddSerilog()).As<ILoggerFactory>();
            builder.Register((c, p) => GetLogger(c.Resolve<IObserver<LogEvent>>()));

            builder.RegisterModule<LexerModule>();
            builder.RegisterModule<ParserModule>();

        }

        public static ILogger GetLogger(IObserver<LogEvent> type)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Observers(obs => obs.Subscribe(type))
                .CreateLogger();
            return logger;
        }
    }
}