using Autofac;
using Parser.Executor;
using Parser.Precedence;

namespace Parser
{
    public class ParserServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VariableStore>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<RecursiveDescentParser>().As<IParser>();

            builder.RegisterType<BasicExecutor>().As<IExecutor>();
        }
    }
}