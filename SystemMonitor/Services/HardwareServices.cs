using FluentScheduler;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using SystemMonitor.Helper;
using SystemMonitor.Models.SettingsModel;

namespace SystemMonitor.Services
{
    public static class HardwareServicesCallBack
    {
        public delegate void CpuChangeEventHandler(HardwareModel.Hardware4Cpu cpu);

        public delegate void GpuChangeEventHandler(HardwareModel.Hardware4Gpu gpu);

        public delegate void MemoryChangeEventHandler(HardwareModel.Hardware4Memory memory);

        public delegate void NetworkChangeEventHandler(HardwareModel.Hardware4Network network);

        public delegate void StorageChangeEventHandler(HardwareModel.Hardware4Disk disk);

        public delegate void HardwareChangeEventHandler(List<HardwareType> list);

        public static event CpuChangeEventHandler CpuChangeEvent;
        public static event GpuChangeEventHandler GpuAChangeEvent;
        public static event GpuChangeEventHandler GpuNChangeEvent;
        public static event MemoryChangeEventHandler MemoryChangeEvent;
        public static event NetworkChangeEventHandler NetworkChangeEvent;
        public static event StorageChangeEventHandler StorageChangeEvent;
        public static event HardwareChangeEventHandler HardwareChangeEvent;

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

    public class HardwareServices : IJob
    {
        private static IServiceProvider ServicesProvider { get; } = Di.ServiceProvider;

        private static MonitorSettings MonitorSettings { get; } =
            ServicesProvider.GetRequiredService<MonitorSettings>();

        private Computer Computer { get; }
        private UpdateVisitor UpdateVisitor { get; } = new();
        private List<string> ActiveAdapterName { get; } = new();
        private List<HardwareType> EnableHardwareList { get; } = new();
        private bool IsHardwareChange { get; set; } = true;
        private static object Lock { get; } = new();

        public HardwareServices()
        {
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
                foreach (var adapter in adapters.Where(a => a.OperationalStatus == OperationalStatus.Up))
                {
                    ActiveAdapterName.Add(adapter.Name);
                }
            }

            Computer.HardwareAdded += ComputerOnHardwareChange;
            Computer.HardwareRemoved += ComputerOnHardwareChange;
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
                    foreach (var adapter in adapters.Where(a => a.OperationalStatus == OperationalStatus.Up))
                    {
                        ActiveAdapterName.Add(adapter.Name);
                    }
                }

                //JobManager.JobEnd += JobManagerOnJobEnd;
            }
        }

        //private void JobManagerOnJobEnd(JobEndInfo obj)
        //{
        //    lock (Lock)
        //    {
        //        Debug.WriteLine("JobManager.JobEnd -= JobManagerOnJobEnd;\t\t" + obj.Name);
        //        JobManager.JobEnd -= JobManagerOnJobEnd;
        //        IsHardwareChange = true;
        //        EnableHardwareList.Clear();
        //    }
        //}

        public void Execute()
        {
            lock (Lock)
            {
                //Debug.WriteLine(DateTime.Now.ToString("O"));
                Computer.Accept(UpdateVisitor);

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.Cpu).Select((t, i) =>
                {
                    var cpuClock = t.Sensors
                        .Where(s => s.SensorType == SensorType.Clock && s.Name.ToUpper().Contains("CORE"))
                        .Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors
                        .Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var load = t.Sensors
                        .Where(s => s.SensorType == SensorType.Load && s.Name.ToUpper().Contains("TOTAL"))
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
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnCpuChange(m);
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
                            !s.Name.ToUpper().Contains("VIRTUAL")).ToList();
                    var used = data.Where(s => s.Name.ToUpper().Contains("USED"))
                        .Average(a => a.Value) ?? 0f;
                    var available = data.Where(s => s.Name.ToUpper().Contains("AVAILABLE"))
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
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnMemoryChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.GpuAmd).Select((t, i) =>
                {
                    var clock = t.Sensors.Where(s => s.SensorType == SensorType.Clock).ToList();
                    var core = clock.Where(s => s.Name.ToUpper().Contains("CORE")).Average(a => a.Value) ?? 0f;
                    var memory = clock.Where(s => s.Name.ToUpper().Contains("MEMORY")).Average(a => a.Value) ?? 0f;
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var m = new HardwareModel.Hardware4Gpu
                    {
                        GpuClock4Core = core,
                        GpuClock4Memory = memory,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        GpuLoad = load,
                        GpuName = t.Name,
                        GpuTemperatures = temperature,
                        Identifier = t.Identifier
                    };
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnGpuAChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.GpuNvidia).Select((t, i) =>
                {
                    var clock = t.Sensors.Where(s => s.SensorType == SensorType.Clock).ToList();
                    var core = clock.Where(s => s.Name.ToUpper().Contains("CORE")).Average(a => a.Value) ?? 0f;
                    var memory = clock.Where(s => s.Name.ToUpper().Contains("MEMORY")).Average(a => a.Value) ?? 0f;
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).Average(a => a.Value) ?? 0f;
                    var temperature = t.Sensors.Where(s => s.SensorType == SensorType.Temperature)
                        .Average(a => a.Value) ?? -999f;
                    var m = new HardwareModel.Hardware4Gpu
                    {
                        GpuClock4Core = core,
                        GpuClock4Memory = memory,
                        Index = i,
                        IndexString = i == 0 ? "" : i + "",
                        GpuLoad = load,
                        GpuName = t.Name,
                        GpuTemperatures = temperature,
                        Identifier = t.Identifier
                    };
                    if (IsHardwareChange) EnableHardwareList.Add(t.HardwareType);
                    else HardwareServicesCallBack.OnGpuNChange(m);
                    return m;
                }).ToList();

                _ = Computer.Hardware.Where(t => t.HardwareType == HardwareType.Storage).Select((t, i) =>
                {
                    var load = t.Sensors.Where(s => s.SensorType == SensorType.Load).ToList();
                    var throughput = t.Sensors.Where(s => s.SensorType == SensorType.Throughput).ToList();
                    var lUsed = load.Where(s => s.Name.ToUpper().Contains("USED")).Average(a => a.Value) ?? 0f;
                    var lWrite = load.Where(s => s.Name.ToUpper().Contains("WRITE")).Average(a => a.Value) ?? 0f;
                    var lTotal = load.Where(s => s.Name.ToUpper().Contains("TOTAL")).Average(a => a.Value) ?? 0f;
                    var tRead = throughput.Where(s => s.Name.ToUpper().Contains("READ")).Average(a => a.Value) ?? 0f;
                    var tWrite = throughput.Where(s => s.Name.ToUpper().Contains("WRITE")).Average(a => a.Value) ?? 0f;

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
                            var u = throughput.Where(s => s.Name.ToUpper().Contains("UPLOAD")).Average(a => a.Value) ??
                                    0f;
                            var d =
                                throughput.Where(s => s.Name.ToUpper().Contains("DOWNLOAD")).Average(a => a.Value) ??
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

    public class HardwareModel
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
}