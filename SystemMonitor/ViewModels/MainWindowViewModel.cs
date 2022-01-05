using FluentScheduler;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Configuration;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using SystemMonitor.Helper;
using SystemMonitor.Models;
using SystemMonitor.Models.SettingsModel;
using SystemMonitor.Services;
using Resource4Common = LocalizedResources.Localization.Module.Common.Resource;
using Resource4Hardware = LocalizedResources.Localization.Module.Hardware.Resource;

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

        private int _windowsWidth;

        public int WindowsWidth
        {
            get => _windowsWidth;
            set => SetProperty(ref _windowsWidth, value);
        }

        private SolidColorBrush _mainWindowBackground;

        public SolidColorBrush MainWindowBackground
        {
            get => _mainWindowBackground;
            set => SetProperty(ref _mainWindowBackground, value);
        }

        public TrulyObservableCollection<TrulyObservableCollection<DisplayItems>> DisplayItemCollection { get; } = new();

        public DelegateCommand CloseAppCommand { get; }
        public DelegateCommand RestartAppCommand { get; }
        public DelegateCommand ShowSettingsViewCommand { get; }

        //private static LimitList<float> NetworkSpeedList { get; } = new(60);
        //private static float LastMaxY { get; set; } = 100f;
        private MonitorSettings MonitorSettings { get; set; }
        private IConfigurationRoot Configuration { get; }
        private IDialogService DialogService { get; }

        public MainWindowViewModel(HardwareServices hardwareServices, MonitorSettings monitorSettings, IConfigurationRoot configuration, IDialogService dialogService)
        {
            MonitorSettings = monitorSettings;
            Title = "System Monitor";
            DialogService = dialogService;
            Configuration = configuration;

            WindowsWidth = MonitorSettings.WindowsWidth;
            MainWindowBackground = new(Color.FromArgb(
                MonitorSettings.MainWindowBackground[0],
                MonitorSettings.MainWindowBackground[1],
                MonitorSettings.MainWindowBackground[2],
                MonitorSettings.MainWindowBackground[3]));

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
            JobManager.AddJob(hardwareServices, e => e.ToRunEvery(MonitorSettings.LoopInterval).Milliseconds());
            JobManager.Start();
            CloseAppCommand = new DelegateCommand(CloseApp);
            RestartAppCommand = new DelegateCommand(RestartApp);
            ShowSettingsViewCommand = new DelegateCommand(ShowSettingsView);
        }

        #region HardwareChangeEvent

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
                MonitorSettings = Configuration.GetSection("MonitorSettings").Get<MonitorSettings>();
                Debug.WriteLine("HardwareServicesCallBackOnHardwareChangeEvent RunInBackground");

                WindowsWidth = MonitorSettings.WindowsWidth;
                MainWindowBackground = new(Color.FromArgb(
                    MonitorSettings.MainWindowBackground[0],
                    MonitorSettings.MainWindowBackground[1],
                    MonitorSettings.MainWindowBackground[2],
                    MonitorSettings.MainWindowBackground[3]));

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
                            CanvasWidth = viewBase.CanvasWidth,
                            FontSize = viewBase.FontSize,
                            //Item8FontSize = viewBase.FontSize / 1.3
                        };
                        item.NetworkSpeedList.CountLimit = MonitorSettings.MonitorViewSettings.NetworkView.DotDensity + 5;
                        for (int i = 0; i < item.NetworkSpeedList.CountLimit; i++)
                        {
                            item.NetworkSpeedList.Add(0);
                        }

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

                JobManager.Start();
            }).RunInBackground();

            HardwareServicesCallBack.CpuChangeEvent += HardwareServicesCallBack_CpuChangeEvent;
            HardwareServicesCallBack.GpuAChangeEvent += HardwareServicesCallBack_GpuAChangeEvent;
            HardwareServicesCallBack.GpuNChangeEvent += HardwareServicesCallBack_GpuNChangeEvent;
            HardwareServicesCallBack.MemoryChangeEvent += HardwareServicesCallBack_MemoryChangeEvent;
            HardwareServicesCallBack.NetworkChangeEvent += HardwareServicesCallBack_NetworkChangeEvent;
            HardwareServicesCallBack.StorageChangeEvent += HardwareServicesCallBack_StorageChangeEvent;
        }

        #endregion

        #region HardwareServicesCallBack

        private void HardwareServicesCallBack_CpuChangeEvent(HardwareModel.Hardware4Cpu cpu)
        {
            new Action(() =>
            {
                int index1 = MonitorSettings.HardwareIndex.IndexOf(HardwareType.Cpu);
                if (DisplayItemCollection.Count > index1 &&
                    DisplayItemCollection[index1].Count > cpu.Index &&
                    DisplayItemCollection[index1][cpu.Index] is { } item)
                {
                    item.Name = Resource4Hardware.CPU;
                    item.Index = cpu.IndexString;
                    item.Identifier = cpu.Identifier;
                    item.Item1 = $"{Convert.ToInt32(cpu.CpuLoad)}%";
                    item.Item2 = $"{CommonHelper.GHzConvert(cpu.CpuClock, out var unit)}{unit}";
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
                    item.Name = Resource4Hardware.GPU;
                    item.Index = gpu.IndexString;
                    item.Identifier = gpu.Identifier;
                    item.Item1 = $"{Convert.ToInt32(gpu.GpuLoad)}%";
                    item.Item2 = Math.Abs(gpu.GpuTemperatures - -999) <= 0
                        ? ""
                        : $"{Convert.ToInt32(gpu.GpuTemperatures)}℃";
                    item.Item3 = $"FPS {gpu.FullscreenFPS}";
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
                    item.Name = Resource4Hardware.GPU;
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
                    item.Name = Resource4Hardware.Memory;
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
                        dSpeed = CommonHelper.FormatBytes(
                            network.NetworkThroughput4DownloadSpeed,
                            out unitD,
                            out var maxUnitIndex);
                        uSpeed = CommonHelper.FormatBytes(
                            network.NetworkThroughput4UploadSpeed,
                            out unitU,
                            out _,
                            maxUnitIndex);
                    }
                    else
                    {
                        uSpeed = CommonHelper.FormatBytes(
                            network.NetworkThroughput4UploadSpeed,
                            out unitU,
                            out var maxUnitIndex);
                        dSpeed = CommonHelper.FormatBytes(
                            network.NetworkThroughput4DownloadSpeed,
                            out unitD,
                            out _,
                            maxUnitIndex);
                    }

                    item.NetworkSpeedList.Insert(0, network.NetworkThroughput4DownloadSpeed);
                    item.Index = network.IndexString;
                    item.Name = $"{network.NetworkName}";
                    item.Item1 = $"{Resource4Common.Send}: {uSpeed}"; // $"发送: {uSpeed} {unitU}"
                    item.Item2 = $"{Resource4Common.Receive}: {dSpeed} {unitD}";
                    item.PointData = network.NetworkThroughput4DownloadSpeed;// Convert.ToInt32(dSpeed);
                    item.CloneBrush();
                    item.PointCollection.InsertAndMove(item);
                    item.LastMaxY = item.InsertAndMove();
                    //item.Item8 = $"{Math.Ceiling(CommonHelper.FormatBytes(LastMaxY, out string unitD2, out _))} {unitD2}";
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
                    item.Name = $"{Resource4Hardware.Disk} {disk.DiskName}";
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

        #endregion

        #region 通知栏右键菜单

        /// <summary>
        /// 退出程序
        /// </summary>
        private void CloseApp()
        {
            JobManager.Stop();
            HardwareServicesCallBack.CpuChangeEvent -= HardwareServicesCallBack_CpuChangeEvent;
            HardwareServicesCallBack.GpuAChangeEvent -= HardwareServicesCallBack_GpuAChangeEvent;
            HardwareServicesCallBack.GpuNChangeEvent -= HardwareServicesCallBack_GpuNChangeEvent;
            HardwareServicesCallBack.MemoryChangeEvent -= HardwareServicesCallBack_MemoryChangeEvent;
            HardwareServicesCallBack.NetworkChangeEvent -= HardwareServicesCallBack_NetworkChangeEvent;
            HardwareServicesCallBack.StorageChangeEvent -= HardwareServicesCallBack_StorageChangeEvent;
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 重启程序
        /// </summary>
        private void RestartApp()
        {
            string appExePath = Regex.Replace(Application.ResourceAssembly.Location, @"\.([D|d][L|l]{2})$", ".exe");
            Process.Start(appExePath);
            CloseApp();
        }

        /// <summary>
        /// 显示设置页面
        /// </summary>
        private void ShowSettingsView()
        {
            DialogService.ShowDialog(nameof(Views.Settings), new DialogParameters(), r =>
            {
                if(r.Result == ButtonResult.Retry)
                {
                    ShowSettingsView();
                }
                Debug.WriteLine(r.Result.ToString());
            });
        }

        #endregion
    }
}