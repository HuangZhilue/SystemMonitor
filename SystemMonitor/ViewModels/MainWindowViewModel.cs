using FluentScheduler;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.DependencyInjection;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SystemMonitor.Helper;
using SystemMonitor.Models;
using SystemMonitor.Models.SettingsModel;
using SystemMonitor.Services;

namespace SystemMonitor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        //public DelegateCommand Click { get; }

        public TrulyObservableCollection<TrulyObservableCollection<DisplayItems>> DisplayItemCollection { get; set; } =
            new();

        /// <summary>
        /// 0"", 1"Kbps", 2"Mbps", 3"Gbps", 4"Tbps", 5"Pbps"
        /// </summary>
        private static string[] Units { get; } = {"", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps"};

        private static double Mod { get; } = 1024.0;

        //private static DisplayItems Data { get; } = new();
        private static Vector Vector { get; } = new(1, 0);

        //private static LimitList<double> DownSpeedList { get; set; } = new();
        private static IServiceProvider ServicesProvider { get; } = Di.ServiceProvider;

        private static MonitorSettings MonitorSettings { get; } =
            ServicesProvider.GetRequiredService<MonitorSettings>();

        //private static List<bool> IsFirstTimes { get; } = new() {true, true, true, true, true};

        public MainWindowViewModel()
        {
            Title = "System Monitor";

            if (MonitorSettings.HardwareIndex.Count <= 0)
            {
                MonitorSettings.HardwareIndex.Add(HardwareType.Cpu);
                MonitorSettings.HardwareIndex.Add(HardwareType.Memory);
                MonitorSettings.HardwareIndex.Add(HardwareType.Storage);
                MonitorSettings.HardwareIndex.Add(HardwareType.Network);
                MonitorSettings.HardwareIndex.Add(HardwareType.GpuAmd);
                MonitorSettings.HardwareIndex.Add(HardwareType.GpuNvidia);
                MonitorSettings.Save2Json();
            }

            HardwareServicesCallBack.HardwareChangeEvent += HardwareServicesCallBackOnHardwareChangeEvent;
            JobManager.AddJob(ServicesProvider.GetRequiredService<HardwareServices>(),
                e => e.ToRunEvery(MonitorSettings.LoopInterval).Milliseconds());
            JobManager.Start();
        }

        private void HardwareServicesCallBackOnHardwareChangeEvent(List<HardwareType> list)
        {
            Debug.WriteLine("HardwareServicesCallBackOnHardwareChangeEvent");
            JobManager.Stop();
            HardwareServicesCallBack.CpuChangeEvent -= HardwareServicesCallBack_CpuChangeEvent;
            HardwareServicesCallBack.GpuAChangeEvent -= HardwareServicesCallBack_GpuAChangeEvent;
            HardwareServicesCallBack.GpuNChangeEvent -= HardwareServicesCallBack_GpuNChangeEvent;
            HardwareServicesCallBack.MemoryChangeEvent -= HardwareServicesCallBack_MemoryChangeEvent;
            HardwareServicesCallBack.NetworkChangeEvent -= HardwareServicesCallBack_NetworkChangeEvent;
            HardwareServicesCallBack.StorageChangeEvent -= HardwareServicesCallBack_StorageChangeEvent;
            new Action(() =>
            {
                Debug.WriteLine("HardwareServicesCallBackOnHardwareChangeEvent RunInBackground");
                DisplayItemCollection.Clear();
                MonitorSettings.HardwareIndex.ForEach(i =>
                {
                    var list2 = new TrulyObservableCollection<DisplayItems>();
                    list.Where(l => l == i).ToList().ForEach(_ => { list2.Add(new DisplayItems()); });
                    if (list2.Count > 0) DisplayItemCollection.Add(list2);
                });
                if (DisplayItemCollection.Count != MonitorSettings.HardwareIndex.Count)
                {
                    //MonitorSettings.HardwareIndex.Clear();
                    var l1 = MonitorSettings.HardwareIndex.FindAll(a => !list.Exists(b => b == a));
                    MonitorSettings.HardwareIndex.RemoveAll(a => l1.Exists(b => b == a));
                    MonitorSettings.HardwareIndex.AddRange(l1);
                    MonitorSettings.Save2Json();
                }
                JobManager.Start();
            }).RunInBackground();

            HardwareServicesCallBack.CpuChangeEvent += HardwareServicesCallBack_CpuChangeEvent;
            HardwareServicesCallBack.GpuAChangeEvent += HardwareServicesCallBack_GpuAChangeEvent;
            HardwareServicesCallBack.GpuNChangeEvent += HardwareServicesCallBack_GpuNChangeEvent;
            HardwareServicesCallBack.MemoryChangeEvent += HardwareServicesCallBack_MemoryChangeEvent;
            HardwareServicesCallBack.NetworkChangeEvent += HardwareServicesCallBack_NetworkChangeEvent;
            HardwareServicesCallBack.StorageChangeEvent += HardwareServicesCallBack_StorageChangeEvent;
        }

        private void HardwareServicesCallBack_CpuChangeEvent(HardwareModel.Hardware4Cpu cpu)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Cpu);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > cpu.Index &&
                    DisplayItemCollection[index1][cpu.Index] is { } item)
                {
                    item.Name = "CPU";
                    item.Index = cpu.IndexString;
                    item.Identifier = cpu.Identifier;
                    item.Item1 = $"{Convert.ToInt32(cpu.CpuLoad)}%";
                    item.Item2 = $"{GHzConvert(cpu.CpuClock, out var unit)}{unit}";
                    item.Item3 = Math.Abs(cpu.CpuTemperatures - -999) <= 0
                        ? ""
                        : $"{Convert.ToInt32(cpu.CpuTemperatures)}℃";
                    item.PointData = Convert.ToInt32(cpu.CpuLoad);
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.CpuView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.CpuView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.CpuView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.CpuView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (CPU)");
            }).RunInBackground();
        }

        private void HardwareServicesCallBack_GpuAChangeEvent(HardwareModel.Hardware4Gpu gpu)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.GpuAmd);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > gpu.Index &&
                    DisplayItemCollection[index1][gpu.Index] is { } item)
                {
                    item.Name = "GPU";
                    item.Index = gpu.IndexString;
                    item.Identifier = gpu.Identifier;
                    item.Item1 = $"{Convert.ToInt32(gpu.GpuLoad)}%";
                    item.Item2 = Math.Abs(gpu.GpuTemperatures - -999) <= 0
                        ? ""
                        : $"{Convert.ToInt32(gpu.GpuTemperatures)}℃";
                    item.PointData = Convert.ToInt32(gpu.GpuLoad);
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (GPU_A)");
            }).RunInBackground();
        }

        private void HardwareServicesCallBack_GpuNChangeEvent(HardwareModel.Hardware4Gpu gpu)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.GpuNvidia);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > gpu.Index &&
                    DisplayItemCollection[index1][gpu.Index] is { } item)
                {
                    item.Name = "GPU";
                    item.Index = gpu.IndexString;
                    item.Identifier = gpu.Identifier;
                    item.Item1 = $"{Convert.ToInt32(gpu.GpuLoad)}%";
                    item.Item2 = Math.Abs(gpu.GpuTemperatures - -999) <= 0
                        ? ""
                        : $"{Convert.ToInt32(gpu.GpuTemperatures)}℃";
                    item.PointData = Convert.ToInt32(gpu.GpuLoad);
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (GPU_N)");
            }).RunInBackground();
        }

        private void HardwareServicesCallBack_MemoryChangeEvent(HardwareModel.Hardware4Memory memory)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Memory);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > memory.Index &&
                    DisplayItemCollection[index1][memory.Index] is { } item)
                {
                    item.Name = "内存";
                    item.Index = memory.IndexString;
                    item.Identifier = memory.Identifier;
                    item.Item1 = $"{memory.MemoryUsedDisplay}";
                    item.Item2 = $"{Convert.ToInt32(memory.MemoryLoad)}%";
                    item.PointData = Convert.ToInt32(memory.MemoryLoad);
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (Memory)");
            }).RunInBackground();
        }

        private void HardwareServicesCallBack_NetworkChangeEvent(HardwareModel.Hardware4Network network)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Network);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > network.Index &&
                    DisplayItemCollection[index1][network.Index] is { } item)
                {
                    double dSpeed;
                    double uSpeed;
                    string unitD;
                    string unitU;
                    if (network.NetworkThroughput4DownloadSpeed >
                        network.NetworkThroughput4UploadSpeed)
                    {
                        dSpeed = FormatBytes(
                            network.NetworkThroughput4DownloadSpeed,
                            out unitD,
                            out var maxUnitIndex);
                        uSpeed = FormatBytes(
                            network.NetworkThroughput4UploadSpeed,
                            out unitU,
                            out _,
                            maxUnitIndex);
                    }
                    else
                    {
                        uSpeed = FormatBytes(
                            network.NetworkThroughput4UploadSpeed,
                            out unitU,
                            out var maxUnitIndex);
                        dSpeed = FormatBytes(
                            network.NetworkThroughput4DownloadSpeed,
                            out unitD,
                            out _,
                            maxUnitIndex);
                    }

                    item.Index = network.IndexString;
                    item.Name = $"{network.NetworkName}";
                    item.Item1 = $"发送: {uSpeed}"; // $"发送: {uSpeed} {unitU}"
                    item.Item2 = $"接收: {dSpeed} {unitD}";
                    item.PointData = Convert.ToInt32(dSpeed);
                    item.MaxPointData = 100;
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (Network)");
            }).RunInBackground();
        }

        private void HardwareServicesCallBack_StorageChangeEvent(HardwareModel.Hardware4Disk disk)
        {
            new Action(() =>
            {
                var index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Storage);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > disk.Index &&
                    DisplayItemCollection[index1][disk.Index] is { } item)
                {
                    item.Name = $"磁盘 {disk.DiskName}";
                    item.Index = disk.IndexString;
                    item.Identifier = disk.Identifier;
                    item.Item1 = $"{Convert.ToInt32(disk.DiskLoad4TotalActivity)}%";
                    item.PointData = Convert.ToInt32(disk.DiskLoad4TotalActivity);
                    item.PointCollection.Insert(1,
                        new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
                    item.StrokeBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[0],
                            MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[1],
                            MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[2],
                            MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[3]));
                    item.FillBrush = new SolidColorBrush(
                        Color.FromArgb(
                            MonitorSettings.MonitorViewSettings.StorageView.FillBrush[0],
                            MonitorSettings.MonitorViewSettings.StorageView.FillBrush[1],
                            MonitorSettings.MonitorViewSettings.StorageView.FillBrush[2],
                            MonitorSettings.MonitorViewSettings.StorageView.FillBrush[3]));
                    item.PointCollection.ChangePoints(Vector, 1);
                }
                else throw new Exception("Out of HardwareIndex (Storage)");
            }).RunInBackground();
        }

        //private void Timer99_Tick(object sender, EventArgs e)
        //{
        //    //Computer.Accept(UpdateVisitor);
        //    DataModel.PauseObservable();
        //    var cpuIndex = 0;
        //    var gpuIndex = 0;
        //    var diskIndex = 0;
        //    var memoryIndex = 0;
        //    var networkIndex = 0;
        //    foreach (var hardware in Computer.Hardware)
        //    {
        //        Data.ClearData();
        //        switch (hardware.HardwareType)
        //        {
        //            case HardwareType.Cpu:
        //            {
        //                //CpuModel.Model.CpuName = hardware.Name;
        //                //CpuModel.Model.CpuClocks.Clear();
        //                //foreach (var sensor in hardware.Sensors)
        //                //{
        //                //    switch (sensor.SensorType)
        //                //    {
        //                //        case SensorType.Clock:
        //                //            if (sensor.Name.ToUpper().Contains("CORE"))
        //                //                CpuModel.Model.CpuClocks.Add(sensor.Value ?? 0);
        //                //            break;
        //                //        case SensorType.Temperature:
        //                //            CpuModel.Model.CpuTemperatures = sensor.Value ?? 0;
        //                //            break;
        //                //        case SensorType.Load:
        //                //            if (sensor.Name.ToUpper().Contains("TOTAL"))
        //                //                CpuModel.Model.CpuLoad = sensor.Value ?? 0;
        //                //            break;
        //                //    }
        //                //}

        //                //Data.Index = cpuIndex++ == 0 ? "" : $"{cpuIndex}";
        //                //Data.Name = "CPU";
        //                //Data.Item1 = $"{Convert.ToInt32(CpuModel.Model.CpuLoad)}%";
        //                //Data.Item2 = $"{GHzConvert(CpuModel.Model.CpuClock, out var unit)}{unit}";
        //                //Data.Item3 = Math.Abs(CpuModel.Model.CpuTemperatures - -999) <= 0
        //                //    ? ""
        //                //    : $"{Convert.ToInt32(CpuModel.Model.CpuTemperatures)}℃";
        //                //Data.PointData = Convert.ToInt32(CpuModel.Model.CpuLoad);
        //                //Data.FillBrush = CpuModel.Model.FillBrush;
        //                //Data.StrokeBrush = CpuModel.Model.StrokeBrush;
        //            }
        //                break;
        //            case HardwareType.Memory:
        //            {
        //                foreach (var sensor in hardware.Sensors)
        //                {
        //                    switch (sensor.SensorType)
        //                    {
        //                        case SensorType.Load:
        //                            MemoryModel.Model.MemoryLoad = sensor.Value ?? 0;
        //                            break;
        //                        case SensorType.Data:
        //                            if (sensor.Name.ToUpper().Contains("USED") &&
        //                                !sensor.Name.ToUpper().Contains("VIRTUAL"))
        //                            {
        //                                MemoryModel.Model.MemoryUsed = sensor.Value ?? 0;
        //                            }
        //                            else if (sensor.Name.ToUpper().Contains("AVAILABLE") &&
        //                                     !sensor.Name.ToUpper().Contains("VIRTUAL"))
        //                            {
        //                                MemoryModel.Model.MemoryAvailable = sensor.Value ?? 0;
        //                            }

        //                            break;
        //                    }
        //                }

        //                Data.Index = memoryIndex++ == 0 ? "" : $"{memoryIndex}";
        //                Data.Name = "内存";
        //                Data.Item1 = $"{MemoryModel.Model.MemoryUsedDisplay}";
        //                Data.Item2 = $"{Convert.ToInt32(MemoryModel.Model.MemoryLoad)}%";
        //                Data.PointData = Convert.ToInt32(MemoryModel.Model.MemoryLoad);
        //                Data.FillBrush = MemoryModel.Model.FillBrush;
        //                Data.StrokeBrush = MemoryModel.Model.StrokeBrush;
        //            }
        //                break;
        //            case HardwareType.GpuNvidia:
        //            case HardwareType.GpuAmd:
        //                //break;
        //            {
        //                GpuModel.Model.GpuName = hardware.Name;
        //                foreach (var sensor in hardware.Sensors)
        //                {
        //                    switch (sensor.SensorType)
        //                    {
        //                        case SensorType.Clock:
        //                            if (sensor.Name.ToUpper().Contains("CORE"))
        //                            {
        //                                GpuModel.Model.GpuClock4Core = sensor.Value ?? 0;
        //                            }
        //                            else if (sensor.Name.ToUpper().Contains("MEMORY"))
        //                            {
        //                                GpuModel.Model.GpuClock4Memory = sensor.Value ?? 0;
        //                            }

        //                            break;
        //                        case SensorType.Temperature:
        //                            GpuModel.Model.GpuTemperatures = sensor.Value ?? 0;
        //                            break;
        //                        case SensorType.Load:
        //                            GpuModel.Model.GpuLoad = sensor.Value ?? 0;
        //                            break;
        //                    }
        //                }

        //                Data.Index = gpuIndex++ == 0 ? "" : $"{gpuIndex}";
        //                Data.Name = "GPU";
        //                Data.Item1 = $"{Convert.ToInt32(GpuModel.Model.GpuLoad)}%";
        //                Data.Item2 = Math.Abs(GpuModel.Model.GpuTemperatures - -999) <= 0
        //                    ? ""
        //                    : $"{Convert.ToInt32(GpuModel.Model.GpuTemperatures)}℃";
        //                Data.PointData = Convert.ToInt32(GpuModel.Model.GpuLoad);
        //                Data.FillBrush = GpuModel.Model.FillBrush;
        //                Data.StrokeBrush = GpuModel.Model.StrokeBrush;
        //            }
        //                break;
        //            case HardwareType.Storage:
        //                //break;
        //                DiskModel.Model.DiskName = hardware.Name;
        //                foreach (var sensor in hardware.Sensors)
        //                {
        //                    switch (sensor.SensorType)
        //                    {
        //                        case SensorType.Load:
        //                            if (sensor.Name.ToUpper().Contains("USED"))
        //                            {
        //                                DiskModel.Model.DiskLoad4UsedSpace = sensor.Value ?? 0;
        //                            }
        //                            else if (sensor.Name.ToUpper().Contains("WRITE"))
        //                            {
        //                                DiskModel.Model.DiskLoad4WriteActivity = sensor.Value ?? 0;
        //                            }
        //                            else if (sensor.Name.ToUpper().Contains("TOTAL"))
        //                            {
        //                                DiskModel.Model.DiskLoad4TotalActivity = sensor.Value ?? 0;
        //                            }

        //                            break;
        //                        case SensorType.Throughput:
        //                            if (sensor.Name.ToUpper().Contains("READ"))
        //                            {
        //                                DiskModel.Model.DiskThroughput4ReadRate = sensor.Value ?? 0;
        //                            }
        //                            else if (sensor.Name.ToUpper().Contains("WRITE"))
        //                            {
        //                                DiskModel.Model.DiskThroughput4WriteRate = sensor.Value ?? 0;
        //                            }

        //                            break;
        //                    }
        //                }

        //                Data.Index = diskIndex++ == 0 ? "" : $"{diskIndex}";
        //                Data.Name = $"磁盘 {DiskModel.Model.DiskName}";
        //                Data.Item1 = $"{Convert.ToInt32(DiskModel.Model.DiskLoad4TotalActivity)}%";
        //                Data.PointData = Convert.ToInt32(DiskModel.Model.DiskLoad4TotalActivity);
        //                Data.FillBrush = DiskModel.Model.FillBrush;
        //                Data.StrokeBrush = DiskModel.Model.StrokeBrush;

        //                break;
        //            case HardwareType.Network:
        //                //break;
        //                if (ActiveAdapterName.Any(a => a.Trim() == hardware.Name.Trim()))
        //                {
        //                    NetworkModel.Model.NetworkName = hardware.Name;
        //                    NetworkModel.Model.IsActive = true;
        //                    foreach (var sensor in hardware.Sensors)
        //                    {
        //                        switch (sensor.SensorType)
        //                        {
        //                            case SensorType.Load:
        //                                if (sensor.Name.ToUpper().Contains("NETWORK UTILIZATION"))
        //                                {
        //                                    NetworkModel.Model.NetworkLoad = sensor.Value ?? 0;
        //                                }

        //                                break;
        //                            case SensorType.Throughput:
        //                                if (sensor.Name.ToUpper().Contains("UPLOAD"))
        //                                {
        //                                    NetworkModel.Model.NetworkThroughput4UploadSpeed = sensor.Value ?? 0;
        //                                }
        //                                else if (sensor.Name.ToUpper().Contains("DOWNLOAD"))
        //                                {
        //                                    NetworkModel.Model.NetworkThroughput4DownloadSpeed = sensor.Value ?? 0;
        //                                    DownSpeedList.Add(NetworkModel.Model.NetworkThroughput4DownloadSpeed);
        //                                }

        //                                break;
        //                        }
        //                    }

        //                    double dSpeed;
        //                    double uSpeed;
        //                    string unitD;
        //                    string unitU;
        //                    if (NetworkModel.Model.NetworkThroughput4DownloadSpeed > NetworkModel.Model.NetworkThroughput4UploadSpeed)
        //                    {
        //                        dSpeed = FormatBytes(
        //                            NetworkModel.Model.NetworkThroughput4DownloadSpeed,
        //                            out unitD,
        //                            out var maxUnitIndex);
        //                        uSpeed = FormatBytes(
        //                            NetworkModel.Model.NetworkThroughput4UploadSpeed,
        //                            out unitU,
        //                            out _,
        //                            maxUnitIndex);
        //                    }
        //                    else
        //                    {
        //                        uSpeed = FormatBytes(
        //                            NetworkModel.Model.NetworkThroughput4UploadSpeed,
        //                            out unitU,
        //                            out var maxUnitIndex);
        //                        dSpeed = FormatBytes(
        //                            NetworkModel.Model.NetworkThroughput4DownloadSpeed,
        //                            out unitD,
        //                            out _,
        //                            maxUnitIndex);
        //                    }

        //                    Data.Index = networkIndex++ == 0 ? "" : $"{networkIndex}";
        //                    Data.Name = $"{NetworkModel.Model.NetworkName}";
        //                    Data.Item1 = $"发送: {uSpeed}";   // $"发送: {uSpeed} {unitU}"
        //                    Data.Item2 = $"接收: {dSpeed} {unitD}";
        //                    Data.PointData = Convert.ToInt32(dSpeed);
        //                    Data.MaxPointData = 100;
        //                    Data.FillBrush = NetworkModel.Model.FillBrush;
        //                    Data.StrokeBrush = NetworkModel.Model.StrokeBrush;
        //                }

        //                break;
        //        }

        //        if (!string.IsNullOrWhiteSpace(Data.Name))
        //            AddOrSet(Data);
        //    }

        //    DataModel.Observable();
        //}

        private static string GHzConvert(float value, out string unit)
        {
            var date = value / 1024;
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
            var i = 0;
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

        public double ChangeNumToPoint(double num, double maxNum)
        {
            return num * (50 / maxNum);
        }

        public int GetMaxPoint(double num)
        {
            return num switch
            {
                <= 5 => 5,
                <= 10 => 10,
                <= 20 => 20,
                <= 50 => 50,
                <= 100 => 100,
                <= 200 => 200,
                <= 500 => 500,
                <= 800 => 800,
                <= 1024 => 1024,
                _ => 1024
            };
        }

        //public void AddOrSet(DisplayItems item)
        //{
        //    if (DataModel.Models.FirstOrDefault(m => m.Name == item.Name && m.Index == item.Index) is { } item1)
        //    {
        //        item1.Item1 = item.Item1;
        //        item1.Item2 = item.Item2;
        //        item1.Item3 = item.Item3;
        //        item1.Item4 = item.Item4;
        //        item1.Item5 = item.Item5;
        //        item1.Item6 = item.Item6;
        //        item1.Item7 = item.Item7;
        //        item1.Item8 = item.Item8;

        //        //for (var i = 0; i < MaxPointCount - 1; i++)
        //        //{
        //        item1.PointCollection.ChangePoints(Vector, 1);
        //        //}

        //        item1.PointCollection.Insert(1,
        //            new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
        //        if (item1.PointCollection.Count > 61) item1.PointCollection.RemoveAt(item1.PointCollection.Count - 2);
        //    }
        //    else
        //    {
        //        var item2 = new DisplayItems
        //        {
        //            FillBrush = item.FillBrush,
        //            StrokeBrush = item.StrokeBrush,
        //            Index = item.Index,
        //            Item1 = item.Item1,
        //            Item2 = item.Item2,
        //            Item3 = item.Item3,
        //            Item4 = item.Item4,
        //            Item5 = item.Item5,
        //            Item6 = item.Item6,
        //            Item7 = item.Item7,
        //            Item8 = item.Item8,
        //            Name = item.Name
        //        };

        //        item2.PointCollection.Add(new Point(0, 0));
        //        for (var i = 0; i <= 60; i++)
        //        {
        //            item2.PointCollection.Add(new Point(i, 0));
        //        }
        //        item2.PointCollection.Add(new Point(60, 0));

        //        item2.PointCollection.Insert(1,
        //            new Point(0, ChangeNumToPoint(item.PointData, item.MaxPointData)));
        //        DataModel.Models.Add(item2);
        //    }
        //}
    }
}