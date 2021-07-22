using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace SystemMonitor.Helper
{
    public static class AppSettingsHelper
    {
        public static JObject Configuration2JObject(this IConfigurationRoot configuration)
        {
            JObject obj = new();

            foreach (IConfigurationSection child in configuration.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            return obj;
        }

        private static JToken Serialize(IConfiguration config)
        {
            JObject obj = new();
            foreach (IConfigurationSection child in config.GetChildren())
            {
                Debug.WriteLine(child.Key);
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
            {
                Debug.WriteLine(section.Value);
                return new JValue(section.Value);
            }

            return obj;
        }
    }
}