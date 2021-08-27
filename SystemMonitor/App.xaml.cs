using Prism.Ioc;
using System.Windows;
using SystemMonitor.ViewModels;
using SystemMonitor.Views;
using Prism.Modularity;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemMonitor.Services;
using SystemMonitor.Models.SettingsModel;

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
            //TaskbarIcon = (TaskbarIcon)new App().FindResource("MyNotifyIcon");

            IConfigurationRoot Configuration = new ConfigurationBuilder()
                .AddJsonFile("AppSettings.json", true, true)
                .Build();
            MonitorSettings monitorSettings = Configuration.GetSection("MonitorSettings").Get<MonitorSettings>();
            monitorSettings.Configuration = Configuration;

            //ServiceProvider ServiceProvider = new ServiceCollection()
            //    .AddSingleton(typeof(IConfigurationRoot), Configuration)
            //    .AddTransient<HardwareServices>()
            //    .AddSingleton(Configuration.GetSection("MonitorSettings").Get<MonitorSettings>())
            //    .AddSingleton((TaskbarIcon)FindResource("MyNotifyIcon"))
            //    .BuildServiceProvider();

            _ = containerRegistry
                .RegisterInstance(Configuration)
                .RegisterScoped<HardwareServices>()
                .RegisterInstance(monitorSettings)
                .RegisterInstance((TaskbarIcon)FindResource("MyNotifyIcon"));
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
        }
    }
}
