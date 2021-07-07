using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SystemMonitor.Helper
{
    public static class AppSettingsHelper
    {
        public static void Save2Json(this IConfigurationRoot configuration)
        {
            var obj = new JObject();
            foreach (var child in configuration.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            var filePath = AppContext.BaseDirectory + "AppSettings.json";

            using var writer = new StreamWriter(filePath);
            using var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
            obj.WriteTo(jsonWriter);
        }

        private static JToken Serialize(IConfiguration config)
        {
            var obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return new JValue(section.Value);

            return obj;
        }
    }
}