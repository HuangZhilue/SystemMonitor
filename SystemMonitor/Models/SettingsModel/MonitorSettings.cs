using System;
using System.Collections.Generic;
using System.IO;
using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SystemMonitor.Models.SettingsModel
{
    public class MonitorSettings
    {
        public int LoopInterval { get; set; }
        public bool IsCpuEnabled { get; set; }
        public bool IsGpuEnabled { get; set; }
        public bool IsMemoryEnabled { get; set; }
        public bool IsNetworkEnabled { get; set; }
        public bool IsStorageEnabled { get; set; }
        public List<HardwareType> HardwareIndex { get; set; } = new();
        public MonitorViewSettings MonitorViewSettings { get; set; }

        public void Save2Json()
        {
            var obj = JObject.Parse(JsonConvert.SerializeObject(this));

            var filePath = AppContext.BaseDirectory + "AppSettings.json";

            using var writer = new StreamWriter(filePath);
            using var jsonWriter = new JsonTextWriter(writer) {Formatting = Formatting.Indented};
            obj.WriteTo(jsonWriter);
        }
    }

    public class MonitorViewSettings
    {
        public MonitorViewBase CpuView { get; set; }
        public MonitorViewBase MemoryView { get; set; }
        public MonitorViewBase StorageView { get; set; }
        public MonitorViewBase NetworkView { get; set; }
        public MonitorViewBase GpuView { get; set; }
    }

    public class MonitorViewBase
    {
        public bool ShowLine { get; set; }
        public List<byte> StrokeBrush { get; set; } = new();
        public List<byte> FillBrush { get; set; } = new();
    }
}