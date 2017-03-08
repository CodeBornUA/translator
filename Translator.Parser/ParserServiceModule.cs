﻿using Autofac;
using Parser.Executor;
using Parser.Precedence;

namespace Parser
{
    public class ParserServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VariableStore>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<PrecedenceParser>().As<IParser>();
        }
    }
}