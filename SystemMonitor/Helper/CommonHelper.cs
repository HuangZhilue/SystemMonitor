using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace SystemMonitor.Helper
{
    public static class CommonHelper
    {
        /// <summary>
        /// 0"", 1"Kbps", 2"Mbps", 3"Gbps", 4"Tbps", 5"Pbps"
        /// </summary>
        private static string[] Units { get; } = { "", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps" };

        private static double Mod { get; } = 1024.0;

        public static string GHzConvert(float value, out string unit)
        {
            float date = value / 1024;
            switch (date)
            {
                case >= 1:
                    unit = "GHz";
                    return date.ToString("0.00");
                case < 1:
                    unit = "MHz";
                    return (date * 1024).ToString("0");
                default:
                    unit = "Hz";
                    return (date * 1024).ToString("0");
            }
        }

        public static double FormatBytes(double size, out string unit, out int maxUnitIndex, int maxUnit = -1)
        {
            size *= 8d;
            int i = 0;
            if (maxUnit < 0)
            {
                while (size >= Mod)
                {
                    size /= Mod;
                    i++;
                }
            }
            else
            {
                while (size >= Mod || i < maxUnit)
                {
                    size /= Mod;
                    i++;
                }
            }

            unit = Units[i];
            maxUnitIndex = i;
            return Math.Round(size, 2);
        }

        public static void RunInBackground(this Action action)
        {
            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
        }
    }
}