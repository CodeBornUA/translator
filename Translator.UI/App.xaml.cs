using System.Windows;
using Autofac;

namespace Translator.UI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IContainer ServiceProvider { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ContainerBuilder();

            builder.RegisterModule<UiModule>();

            ServiceProvider = builder.Build();
        }
    }
}