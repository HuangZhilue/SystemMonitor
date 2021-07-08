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
        private static string[] Units { get; } = { "", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps" };

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

            if (MonitorSettings.HardwareIndex.Count == 0)
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
                    list.Where(l => l == i).ToList().ForEach(_ => list2.Add(new DisplayItems { Visibility = Visibility.Visible }));
                    //if (list2.Count > 0) DisplayItemCollection.Add(list2);
                    //else
                    DisplayItemCollection.Add(list2);
                });
                //if (DisplayItemCollection.Count != MonitorSettings.HardwareIndex.Count)
                //{
                //    //MonitorSettings.HardwareIndex.Clear();
                //    var l1 = MonitorSettings.HardwareIndex.FindAll(a => !list.Exists(b => b == a));
                //    MonitorSettings.HardwareIndex.RemoveAll(a => l1.Exists(b => b == a));
                //    MonitorSettings.HardwareIndex.AddRange(l1);
                //    MonitorSettings.Save2Json();
                //}
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
                else
                {
                    throw new Exception("Out of HardwareIndex (CPU)");
                }
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
                else
                {
                    throw new Exception("Out of HardwareIndex (GPU_A)");
                }
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
                else
                {
                    throw new Exception("Out of HardwareIndex (GPU_N)");
                }
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
                else
                {
                    throw new Exception("Out of HardwareIndex (Memory)");
                }
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
                else
                {
                    throw new Exception("Out of HardwareIndex (Network)");
                }
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
                else
                {
                    throw new Exception("Out of HardwareIndex (Storage)");
                }
            }).RunInBackground();
        }

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
    }
}