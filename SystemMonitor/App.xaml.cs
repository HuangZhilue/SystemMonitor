using Prism.Ioc;
using System.Windows;
using SystemMonitor.ViewModels;
using SystemMonitor.Views;
using Prism.Modularity;

namespace SystemMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {

        }
    }
}
