using FluentScheduler;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using SystemMonitor.Models.SettingsModel;

namespace SystemMonitor.Services
{
    #region HardwareServicesCallBack

    public static class HardwareServicesCallBack
    {
        public delegate void CpuChangeCallback(HardwareModel.Hardware4Cpu cpu);

        public delegate void GpuChangeCallback(HardwareModel.Hardware4Gpu gpu);

        public delegate void MemoryChangeCallback(HardwareModel.Hardware4Memory memory);

        public delegate void NetworkChangeCallback(HardwareModel.Hardware4Network network);

        public delegate void StorageChangeCallback(HardwareModel.Hardware4Disk disk);

        public delegate void HardwareChangeCallback(List<HardwareType> list);

        public static event CpuChangeCallback CpuChangeEvent;
        public static event GpuChangeCallback GpuAChangeEvent;
        public static event GpuChangeCallback GpuNChangeEvent;
        public static event MemoryChangeCallback MemoryChangeEvent;
        public static event NetworkChangeCallback NetworkChangeEvent;
        public static event StorageChangeCallback StorageChangeEvent;
        public static event HardwareChangeCallback HardwareChangeEvent;

        internal static void OnCpuChange(HardwareModel.Hardware4Cpu data)
        {
            CpuChangeEvent?.Invoke(data);
        }

        internal static void OnGpuAChange(HardwareModel.Hardware4Gpu data)
        {
            GpuAChangeEvent?.Invoke(data);
        }

        internal static void OnGpuNChange(HardwareModel.Hardware4Gpu data)
        {
            GpuNChangeEvent?.Invoke(data);
        }

        internal static void OnMemoryChange(HardwareModel.Hardware4Memory data)
        {
            MemoryChangeEvent?.Invoke(data);
        }

        internal static void OnNetworkChange(HardwareModel.Hardware4Network data)
        {
            NetworkChangeEvent?.Invoke(data);
        }

        internal static void OnStorageChange(HardwareModel.Hardware4Disk data)
        {
            StorageChangeEvent?.Invoke(data);
        }

        internal static void OnHardwareChangeEvent(List<HardwareType> list)
        {
            HardwareChangeEvent?.Invoke(list);
        }
    }

    #endregion

    #region HardwareServices

    public class HardwareServices : IJob
    {
        //private static IServiceProvider ServicesProvider { get; } = Di.ServiceProvider;

        private MonitorSettings MonitorSettings { get; } // = ServicesProvider.GetRequiredService<MonitorSettings>();

        private Computer Computer { get; }
        private UpdateVisitor UpdateVisitor { get; } = new();
        private List<string> ActiveAdapterName { get; } = new();
        private List<HardwareType> EnableHardwareList { get; } = new();
        private bool IsHardwareChange { get; set; }
        private static object Lock { get; } = new();

        public HardwareServices(MonitorSettings monitorSettings)
        {
            MonitorSettings = monitorSettings;
            IsHardwareChange = true;
            Computer = new Computer
            {
                IsCpuEnabled = MonitorSettings.IsCpuEnabled,
                IsGpuEnabled = MonitorSettings.IsGpuEnabled,
                IsMemoryEnabled = MonitorSettings.IsMemoryEnabled,
                IsNetworkEnabled = MonitorSettings.IsNetworkEnabled,
                IsStorageEnabled = MonitorSettings.IsStorageEnabled
            };
            Computer.Open();

            if (Computer.IsNetworkEnabled)
            {
                ActiveAdapterName.Clear();
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                ActiveAdapterName.AddRange(
                    adapters.Where(a => a.OperationalStatus == OperationalStatus.Up)
                    .Select(adapter => adapter.Name)
                    );
            }

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            Computer.HardwareAdded += ComputerOnHardwareChange;
            Computer.HardwareRemoved += ComputerOnHardwareChange;
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            lock (Lock)
            {
                Debug.WriteLine("NetworkChange_NetworkAddressChanged");
                IsHardwareChange = true;
                EnableHardwareList.Clear();
                ActiveAdapterName.Clear();
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                ActiveAdapterName.AddRange(
                    adapters.Where(a => a.OperationalStatus == OperationalStatus.Up)
                    .Select(adapter => adapter.Name)
                    );
            }
        }

        private void ComputerOnHardwareChange(IHardware hardware)
        {
            lock (Lock)
            {
                Debug.WriteLine("ComputerOnHardwareChange");
                IsHardwareChange = true;
                EnableHardwareList.Clear();
                if (Computer.IsNetworkEnabled && hardware.HardwareType == HardwareType.Network)
                {
                    ActiveAdapterName.Clear();
                    var adapters = NetworkInterface.GetAllNetworkInterfaces();
                    ActiveAdapterName.AddRange(
                        adapters.Where(a => a.OperationalStatus == OperationalStatus.Up)
                        .Select(adapter => adapter.Name)
                        );
                }
            }
        }

        public void Execute()
        {
            lock (Lock)
            {
                //Debug.WriteLine("Execute");
                Computer.Accept(UpdateVisitor);

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.Cpu).Select((t, i) =>
                {
                    var cpuClock = t.Sensors
                        .Where(s => s.SensorType == SensorType.Clock && s.Name.Contains("CORE", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors
                        .Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var load = t.Sensors
                        .Where(s => s.SensorType == SensorType.Load && s.Name.Contains("TOTAL", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Value) ?? 0f;
                    var m = new HardwareModel.Hardware4Cpu
                    {
                        CpuClock = cpuClock,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        CpuLoad = load,
                        CpuName = t.Name,
                        CpuTemperatures = temperature,
                        Identifier = t.Identifier
                    };
                    if (IsHardwareChange)
                    {
                        EnableHardwareList.Add(t.HardwareType);
                    }
                    else
                    {
                        HardwareServicesCallBack.OnCpuChange(m);
                    }

                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.Memory).Select((t, i) =>
                {
                    var load = t.Sensors
                        .Where(s => s.SensorType == SensorType.Load)
                        .Average(a => a.Value) ?? 0f;
                    var data = t.Sensors
                        .Where(s =>
                            s.SensorType == SensorType.Data &&
                            !s.Name.Contains("VIRTUAL", StringComparison.OrdinalIgnoreCase)).ToList();
                    var used = data.Where(s => s.Name.Contains("USED", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Value) ?? 0f;
                    var available = data.Where(s => s.Name.Contains("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                        .Average(a => a.Value) ?? 0f;
                    var m = new HardwareModel.Hardware4Memory
                    {
                        MemoryAvailable = available,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        MemoryLoad = load,
                        MemoryUsed = used,
                        Identifier = t.Identifier
                    };
                    if (IsHardwareChange)
                    {
                        EnableHardwareList.Add(t.HardwareType);
                    }
                    else
                    {
                        HardwareServicesCallBack.OnMemoryChange(m);
                    }

                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.GpuAmd).Select((t, i) =>
                {
                    var clock = t.Sensors.Where(s => s.SensorType == SensorType.Clock).ToList();
                    var core = clock.Where(s => s.Name.Contains("CORE", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var memory = clock.Where(s => s.Name.Contains("MEMORY", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var fps = t.Sensors.Where(s => s.Name.Contains("Fullscreen FPS", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var m = new HardwareModel.Hardware4Gpu
                    {
                        GpuClock4Core = core,
                        GpuClock4Memory = memory,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        GpuLoad = load,
                        GpuName = t.Name,
                        GpuTemperatures = temperature,
                        Identifier = t.Identifier,
                        FullscreenFPS = fps
                    };
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnGpuAChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.GpuNvidia).Select((t, i) =>
                {
                    var clock = t.Sensors.Where(s => s.SensorType == SensorType.Clock).ToList();
                    var core = clock.Where(s => s.Name.Contains("CORE", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var memory = clock.Where(s => s.Name.Contains("MEMORY", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var fps = t.Sensors.Where(s => s.Name.Contains("Fullscreen FPS", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var m = new HardwareModel.Hardware4Gpu
                    {
                        GpuClock4Core = core,
                        GpuClock4Memory = memory,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        GpuLoad = load,
                        GpuName = t.Name,
                        GpuTemperatures = temperature,
                        Identifier = t.Identifier,
                        FullscreenFPS = fps
                    };
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnGpuNChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.Storage).Select((t, i) =>
                {
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).ToList();
                    var throughput = t.Sensors.Where(s => s.SensorType == SensorType.Throughput).ToList();
                    var lUsed = load.Where(s => s.Name.Contains("USED", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var lWrite = load.Where(s => s.Name.Contains("WRITE", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var lTotal = load.Where(s => s.Name.Contains("TOTAL", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var tRead = throughput.Where(s => s.Name.Contains("READ", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;
                    var tWrite = throughput.Where(s => s.Name.Contains("WRITE", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ?? 0f;

                    var m = new HardwareModel.Hardware4Disk
                    {
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        DiskLoad4TotalActivity = lTotal,
                        DiskLoad4UsedSpace = lUsed,
                        DiskLoad4WriteActivity = lWrite,
                        DiskName = t.Name,
                        DiskThroughput4ReadRate = tRead,
                        DiskThroughput4WriteRate = tWrite,
                        Identifier = t.Identifier
                    };
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnStorageChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t =>
                        t.HardwareType == HardwareType.Network && ActiveAdapterName.Any(a => a.Trim() == t.Name.Trim()))
                    .Select(
                        (t, i) =>
                        {
                            var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).Average(a => a.Value) ??
                                       0f;
                            var throughput = t.Sensors.Where(s => s.SensorType == SensorType.Throughput).ToList();
                            var u = throughput.Where(s => s.Name.Contains("UPLOAD", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ??
                                    0f;
                            var d =
                                throughput.Where(s => s.Name.Contains("DOWNLOAD", StringComparison.OrdinalIgnoreCase)).Average(a => a.Value) ??
                                0f;
                            var m = new HardwareModel.Hardware4Network
                            {
                                IsActive = true,
                                Index = i,
                                IndexString = i == 0 ? "" : i + "",
                                NetworkLoad = load,
                                NetworkName = t.Name,
                                NetworkThroughput4DownloadSpeed = d,
                                NetworkThroughput4UploadSpeed = u,
                                Identifier = t.Identifier
                            };
                            if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                            else HardwareServicesCallBack.OnNetworkChange(m);
                            return m;
                        }).ToList();

                if (!IsHardwareChange) return;
                HardwareServicesCallBack.OnHardwareChangeEvent(EnableHardwareList);
                IsHardwareChange = false;
            }
        }
    }

    #endregion

    #region UpdateVisitor

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware) subHardware.Accept(this);
        }

        public void VisitSensor(ISensor sensor)
        {
        }

        public void VisitParameter(IParameter parameter)
        {
        }
    }

    #endregion

    #region HardwareModel

    public static class HardwareModel
    {
        public class Hardware4Cpu
        {
            public Identifier Identifier { get; set; }
            public int Index { get; set; }
            public string IndexString { get; set; }
            public string CpuName { get; set; }
            public float CpuClock { get; set; }
            public float CpuLoad { get; set; }
            public float CpuTemperatures { get; set; } = -999;
        }

        public class Hardware4Gpu
        {
            public Identifier Identifier { get; set; }
            public int Index { get; set; }
            public string IndexString { get; set; }
            public string GpuName { get; set; }
            public float GpuClock4Core { get; set; }
            public float GpuClock4Memory { get; set; }
            public float GpuLoad { get; set; }
            public float GpuTemperatures { get; set; } = -999;
            public float FullscreenFPS { get; set; }
        }

        public class Hardware4Memory
        {
            public Identifier Identifier { get; set; }
            public int Index { get; set; }
            public string IndexString { get; set; }
            public float MemoryLoad { get; set; }

            //public float MemoryLoad4Virtual { get; set; }
            public float MemoryUsed { get; set; }

            //public float MemoryUsed4Virtual { get; set; }
            public float MemoryAvailable { get; set; }

            //public float MemoryAvailable4Virtual { get; set; }
            public float MemoryTotal => MemoryUsed + MemoryAvailable;
            public string MemoryUsedDisplay => MemoryUsed.ToString("0.0") + "/" + MemoryTotal.ToString("0.0") + "GB";
        }

        public class Hardware4Disk
        {
            public Identifier Identifier { get; set; }
            public int Index { get; set; }
            public string IndexString { get; set; }
            public string DiskName { get; set; }
            public float DiskLoad4UsedSpace { get; set; }
            public float DiskLoad4WriteActivity { get; set; }
            public float DiskLoad4TotalActivity { get; set; }
            public float DiskThroughput4ReadRate { get; set; }
            public float DiskThroughput4WriteRate { get; set; }
        }

        public class Hardware4Network
        {
            public Identifier Identifier { get; set; }
            public int Index { get; set; }
            public string IndexString { get; set; }
            public string NetworkName { get; set; }
            public bool IsActive { get; set; }
            public float NetworkThroughput4UploadSpeed { get; set; }
            public float NetworkThroughput4DownloadSpeed { get; set; }
            public float NetworkLoad { get; set; }
        }
    }

    #endregion
}