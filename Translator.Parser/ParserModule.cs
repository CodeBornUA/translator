using Autofac;
using Parser.Executor;
using Parser.Precedence;
using Parser.Recursive;
using Parser.StateMachine;

namespace Parser
{
    public class ParserModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<VariableStore>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<RecursiveDescentParser>().As<IParser>();

            builder.RegisterType<BasicExecutor>().As<IExecutor>();
        }
    }
}