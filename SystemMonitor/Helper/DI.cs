using System;
using System.Collections.Generic;
using SystemMonitor.Models.SettingsModel;
using SystemMonitor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SystemMonitor.Helper
{
    public static class Di
    {
        public static IServiceProvider ServiceProvider { get; set; }
        public static IConfigurationRoot Configuration { get; set; }

        static Di()
        {
            BuildDi();
        }

        private static void BuildDi()
        {
            // Build configuration
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("AppSettings.json", true, true)
                .Build();

            var monitorSettings = new MonitorSettings();

            Configuration.Bind(nameof(MonitorSettings), monitorSettings);

            ServiceProvider = new ServiceCollection()
                .AddSingleton(typeof(IConfigurationRoot), Configuration)
                .AddTransient<HardwareServices>()
                .AddSingleton(monitorSettings)
                .BuildServiceProvider();
        }
    }
}