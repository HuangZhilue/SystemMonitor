using System;
using System.Collections.Generic;
using System.IO;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SystemMonitor.Helper;

namespace SystemMonitor.Models.SettingsModel
{
    public class MonitorSettings
    {
        private int _loopInterval;

        public int LoopInterval
        {
            get => _loopInterval;
            set => _loopInterval = value < 100 ? 100 : value;
        }
        public int WindowsWidth { get; set; } = 250;
        public bool IsCpuEnabled { get; set; }
        public bool IsGpuEnabled { get; set; }
        public bool IsMemoryEnabled { get; set; }
        public bool IsNetworkEnabled { get; set; }
        public bool IsStorageEnabled { get; set; }
        public List<HardwareType> HardwareIndex { get; set; } = new(); // { HardwareType.Cpu, HardwareType.Memory, HardwareType.Storage, HardwareType.Network, HardwareType.GpuAmd, HardwareType.GpuNvidia };
        public MonitorViewSettings MonitorViewSettings { get; set; }

        [JsonIgnore]
        public IConfigurationRoot Configuration { private get; set; } // = Di.Configuration;

        //public MonitorSettings(IConfigurationRoot configuration)
        //{
        //    Configuration = configuration;
        //}

        public void Save2Json()
        {
            JObject obj = JObject.Parse(JsonConvert.SerializeObject(this));
            JObject cObj = Configuration.Configuration2JObject();
            cObj["MonitorSettings"] = obj;
            string filePath = AppContext.BaseDirectory + "AppSettings.json";
            using StreamWriter writer = new(filePath);
            using JsonTextWriter jsonWriter = new(writer) { Formatting = Formatting.Indented };
            cObj.WriteTo(jsonWriter);
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
        public int DotDensity { get; set; } = 60;
        public int CanvasHeight { get; set; } = 55;
        public int CanvasWidth { get; set; } = 60;
        public bool ShowLine { get; set; }
        public double FontSize { get; set; } = 12d;
        public List<byte> StrokeBrush { get; set; } = new();
        public List<byte> FillBrush { get; set; } = new();
        public List<byte> Background { get; set; } = new() { 125, 0, 0, 0 };
        public List<byte> Foreground { get; set; } = new() { 255, 0, 255, 0 };
    }
}