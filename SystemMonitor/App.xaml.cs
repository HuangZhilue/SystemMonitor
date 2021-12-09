using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using SystemMonitor.Models.SettingsModel;
using SystemMonitor.Services;
using SystemMonitor.ViewModels;
using SystemMonitor.Views;

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
            IConfigurationRoot Configuration = new ConfigurationBuilder()
                .AddJsonFile("AppSettings.json", true, true)
                .Build();
            MonitorSettings monitorSettings = Configuration.GetSection("MonitorSettings").Get<MonitorSettings>();
            monitorSettings.Configuration = Configuration;
            HardwareServices hardwareServices = new(monitorSettings);

            _ = containerRegistry
                .RegisterInstance(Configuration)
                .RegisterInstance(hardwareServices)
                .RegisterInstance(monitorSettings);

            containerRegistry.RegisterDialog<Settings, SettingsViewModel>();
            containerRegistry.RegisterDialog<ColorPicker, ColorPickerViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
        }
    }
}
