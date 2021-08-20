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

        public TrulyObservableCollection<TrulyObservableCollection<DisplayItems>> DisplayItemCollection { get; set; } =
            new();

        /// <summary>
        /// 0"", 1"Kbps", 2"Mbps", 3"Gbps", 4"Tbps", 5"Pbps"
        /// </summary>
        private static string[] Units { get; } = { "", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps" };

        private static double Mod { get; } = 1024.0;

        //private static Vector Vector { get; } = new(1, 0);
        private static bool NeedResetHeight { get; set; }

        private static IServiceProvider ServicesProvider { get; } = Di.ServiceProvider;

        private static MonitorSettings MonitorSettings { get; } =
            ServicesProvider.GetRequiredService<MonitorSettings>();

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
            NeedResetHeight = true;
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
                    MonitorViewBase viewBase = i switch
                    {
                        HardwareType.Cpu => MonitorSettings.MonitorViewSettings.CpuView,
                        HardwareType.Memory => MonitorSettings.MonitorViewSettings.MemoryView,
                        HardwareType.GpuNvidia or HardwareType.GpuAmd => MonitorSettings.MonitorViewSettings.GpuView,
                        HardwareType.Storage => MonitorSettings.MonitorViewSettings.StorageView,
                        HardwareType.Network => MonitorSettings.MonitorViewSettings.NetworkView,
                        _ => throw new Exception("Out of HardwareType")
                    };

                    SolidColorBrush strokeBrush = new(
                        Color.FromArgb(
                            viewBase.StrokeBrush[0],
                            viewBase.StrokeBrush[1],
                            viewBase.StrokeBrush[2],
                            viewBase.StrokeBrush[3]));
                    SolidColorBrush fillBrush = new(
                        Color.FromArgb(
                            viewBase.FillBrush[0],
                            viewBase.FillBrush[1],
                            viewBase.FillBrush[2],
                            viewBase.FillBrush[3]));
                    SolidColorBrush foreground = new(
                        Color.FromArgb(
                            viewBase.Foreground[0],
                            viewBase.Foreground[1],
                            viewBase.Foreground[2],
                            viewBase.Foreground[3]));
                    SolidColorBrush background = new(
                        Color.FromArgb(
                            viewBase.Background[0],
                            viewBase.Background[1],
                            viewBase.Background[2],
                            viewBase.Background[3]));

                    TrulyObservableCollection<DisplayItems> list2 = new();
                    list.Where(l => l == i).ToList()
                    .ForEach(_ =>
                    {
                        DisplayItems item = new()
                        {
                            Visibility = Visibility.Visible,
                            LineVisibility = viewBase.ShowLine ? Visibility.Visible : Visibility.Collapsed,
                            DotDensity = viewBase.DotDensity,
                            StrokeBrush = strokeBrush,
                            FillBrush = fillBrush,
                            Foreground = foreground,
                            Background = background,
                            CanvasHeight = viewBase.CanvasHeight,
                            CanvasWidth = viewBase.CanvasWidth
                        };

                        item.PointCollection.Add(new Point(0, 0));
                        for (int i = 0; i <= viewBase.DotDensity; i++)
                        {
                            item.PointCollection.Add(new Point(i, 0));
                        }
                        item.PointCollection.Add(new Point(viewBase.DotDensity, 0));

                        list2.Add(item);
                    });
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

                NeedResetHeight = true;
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Cpu);
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
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.GpuAmd);
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
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.GpuNvidia);
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
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Memory);
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
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Network);
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
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
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
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Storage);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > disk.Index &&
                    DisplayItemCollection[index1][disk.Index] is { } item)
                {
                    item.Name = $"磁盘 {disk.DiskName}";
                    item.Index = disk.IndexString;
                    item.Identifier = disk.Identifier;
                    item.Item1 = $"{Convert.ToInt32(disk.DiskLoad4TotalActivity)}%";
                    item.PointData = Convert.ToInt32(disk.DiskLoad4TotalActivity);
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
                }
                else
                {
                    throw new Exception("Out of HardwareIndex (Storage)");
                }
            }).RunInBackground();
        }

        private static string GHzConvert(float value, out string unit)
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

        private static double FormatBytes(double size, out string unit, out int maxUnitIndex, int maxUnit = -1)
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