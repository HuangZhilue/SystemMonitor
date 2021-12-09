using FluentScheduler;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SystemMonitor.Helper;
using SystemMonitor.Models;
using SystemMonitor.Models.SettingsModel;
using SystemMonitor.Services;

namespace SystemMonitor.ViewModels
{
    public class SettingsViewModel : BindableBase, IDialogAware
    {
        private string _title = "设置";
        private SettingsModels _selectedSettings;
        private int _loopInterval;
        private int _windowsWidth = 250;
        private bool _isAutoRun;
        private double _opacity = 1d;
        private Visibility _isSaving = Visibility.Hidden;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public SettingsModels SelectedSettings
        {
            get => _selectedSettings;
            set => SetProperty(ref _selectedSettings, value);
        }

        public int LoopInterval
        {
            get => _loopInterval;
            set => SetProperty(ref _loopInterval, value);
        }

        public int WindowsWidth
        {
            get => _windowsWidth;
            set => SetProperty(ref _windowsWidth, value);
        }

        public bool IsAutoRun
        {
            get => _isAutoRun;
            set => SetProperty(ref _isAutoRun, value);
        }

        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        public Visibility IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public TrulyObservableCollection<SettingsModels> SettingsModelList { get; } = new();

        public DelegateCommand<string> CloseDialogCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand RestoreCommand { get; }
        public DelegateCommand<SettingsModels> SelectionChangedCommand { get; }
        public DelegateCommand<SettingsModels> IndexUpChangedCommand { get; }
        public DelegateCommand<SettingsModels> IndexDownChangedCommand { get; }
        public DelegateCommand<string> SelectColorCommand { get; }
        private MonitorSettings MonitorSettings { get; set; }
        private HardwareServices HardwareServices { get; set; }
        private IConfigurationRoot Configuration { get; }
        private static IDialogService DialogService { get; set; }

        public SettingsViewModel(MonitorSettings monitorSettings, HardwareServices hardwareServices, IConfigurationRoot configuration, IDialogService dialogService)
        {
            MonitorSettings = monitorSettings;
            HardwareServices = hardwareServices;
            Configuration = configuration;
            DialogService = dialogService;
            CloseDialogCommand = new DelegateCommand<string>(CloseDialog);
            SelectionChangedCommand = new DelegateCommand<SettingsModels>(SelectionChanged);
            IndexUpChangedCommand = new DelegateCommand<SettingsModels>(IndexUpChanged);
            IndexDownChangedCommand = new DelegateCommand<SettingsModels>(IndexDownChanged);
            SelectColorCommand = new DelegateCommand<string>(SelectColor);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            RestoreCommand = new DelegateCommand(Restore);
            InitModel();
        }

        private void InitModel()
        {
            LoopInterval = MonitorSettings.LoopInterval;
            WindowsWidth = MonitorSettings.WindowsWidth;

            {
                RegistryKey registryKeyk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string regValue = registryKeyk.GetValue(nameof(SystemMonitor))?.ToString();
                if (!string.IsNullOrWhiteSpace(regValue))
                {
                    IsAutoRun = regValue == Environment.ProcessPath;
                }
                else
                {
                    IsAutoRun = false;
                }
            }

            int index = 0;
            SettingsModelList.Clear();
            MonitorSettings.HardwareIndex.ForEach(i =>
            {
                MonitorViewBase viewBase = null;
                bool isEnable = false;
#pragma warning disable IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
                switch (i)
#pragma warning restore IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
                {
                    case HardwareType.Cpu:
                        viewBase = MonitorSettings.MonitorViewSettings.CpuView;
                        isEnable = MonitorSettings.IsCpuEnabled;
                        break;
                    case HardwareType.Memory:
                        viewBase = MonitorSettings.MonitorViewSettings.MemoryView;
                        isEnable = MonitorSettings.IsMemoryEnabled;
                        break;
                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                        viewBase = MonitorSettings.MonitorViewSettings.GpuView;
                        isEnable = MonitorSettings.IsGpuEnabled;
                        break;
                    case HardwareType.Storage:
                        viewBase = MonitorSettings.MonitorViewSettings.StorageView;
                        isEnable = MonitorSettings.IsStorageEnabled;
                        break;
                    case HardwareType.Network:
                        viewBase = MonitorSettings.MonitorViewSettings.NetworkView;
                        isEnable = MonitorSettings.IsNetworkEnabled;
                        break;
                    default:
                        throw new Exception("Out of HardwareType");
                }

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

                SettingsModels item = new()
                {
                    HardwareIndex = index,
                    HardwareType = i,
                    HardwareTypeString = HardType2String(i),
                    IsEnabled = isEnable,
                    DotDensity = viewBase.DotDensity,
                    StrokeBrush = strokeBrush,
                    FillBrush = fillBrush,
                    Foreground = foreground,
                    Background = background,
                    CanvasHeight = viewBase.CanvasHeight,
                    CanvasWidth = viewBase.CanvasWidth,
                    FontSize = viewBase.FontSize,
                    ShowLine = viewBase.ShowLine
                };

                if (item.HardwareTypeString != "显卡" || (item.HardwareTypeString == "显卡" && !SettingsModelList.Any(s => s.HardwareTypeString.Contains("显卡"))))
                {
                    SettingsModelList.Add(item);
                }

                index++;
            });
        }

        #region Save & Cancel & Restore

        private void Save()
        {
            IsSaving = Visibility.Visible;
            Opacity = 0.5d;

            // 因为SettingsModelList中的SolidColorBrush数据存在线程安全问题，暂时通过该方法继续操作
            var settingsModelList = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(SettingsModelList.ToList()), new JsonSerializerSettings() { Context = new System.Runtime.Serialization.StreamingContext() });

            Task.Run(() =>
            {
                //new Action(() =>
                //{
                JobManager.Stop();
                JobManager.AllSchedules.ToList().ForEach(job => job.Disable());
                JobManager.RemoveAllJobs();

                MonitorSettings.LoopInterval = LoopInterval;
                MonitorSettings.WindowsWidth = WindowsWidth;

                {
                    RegistryKey registryKeyk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (IsAutoRun)
                    {
                        registryKeyk.SetValue(nameof(SystemMonitor), Environment.ProcessPath);
                    }
                    else
                    {
                        registryKeyk.DeleteValue(nameof(SystemMonitor));
                    }
                }

                MonitorSettings.HardwareIndex.Clear();
                settingsModelList.ForEach(item =>
                {
                    #region 因为SettingsModelList中的SolidColorBrush数据存在线程安全问题，暂时通过该方法继续操作

                    MonitorSettings.HardwareIndex.Add((HardwareType)item.HardwareType);
                    Color background = (Color)ColorConverter.ConvertFromString(item.Background.ToString());
                    Color foreground = (Color)ColorConverter.ConvertFromString(item.Foreground.ToString());
                    Color strokeBrush = (Color)ColorConverter.ConvertFromString(item.StrokeBrush.ToString());
                    Color fillBrush = (Color)ColorConverter.ConvertFromString(item.FillBrush.ToString());

                    #endregion

#pragma warning disable IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
                    switch (item.HardwareType)
#pragma warning restore IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
                    {
                        case HardwareType.Cpu:
                            MonitorSettings.IsCpuEnabled = item.IsEnabled;
                            MonitorSettings.MonitorViewSettings.CpuView.ShowLine = item.ShowLine;
                            MonitorSettings.MonitorViewSettings.CpuView.DotDensity = item.DotDensity;
                            MonitorSettings.MonitorViewSettings.CpuView.CanvasHeight = item.CanvasHeight;
                            MonitorSettings.MonitorViewSettings.CpuView.CanvasWidth = item.CanvasWidth;
                            MonitorSettings.MonitorViewSettings.CpuView.Background = new() { background.A, background.R, background.G, background.B };
                            MonitorSettings.MonitorViewSettings.CpuView.Foreground = new() { foreground.A, foreground.R, foreground.G, foreground.B };
                            MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush = new() { strokeBrush.A, strokeBrush.R, strokeBrush.G, strokeBrush.B };
                            MonitorSettings.MonitorViewSettings.CpuView.FillBrush = new() { fillBrush.A, fillBrush.R, fillBrush.G, fillBrush.B };
                            break;
                        case HardwareType.Memory:
                            MonitorSettings.IsMemoryEnabled = item.IsEnabled;
                            MonitorSettings.MonitorViewSettings.MemoryView.ShowLine = item.ShowLine;
                            MonitorSettings.MonitorViewSettings.MemoryView.DotDensity = item.DotDensity;
                            MonitorSettings.MonitorViewSettings.MemoryView.CanvasHeight = item.CanvasHeight;
                            MonitorSettings.MonitorViewSettings.MemoryView.CanvasWidth = item.CanvasWidth;
                            MonitorSettings.MonitorViewSettings.MemoryView.Background = new() { background.A, background.R, background.G, background.B };
                            MonitorSettings.MonitorViewSettings.MemoryView.Foreground = new() { foreground.A, foreground.R, foreground.G, foreground.B };
                            MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush = new() { strokeBrush.A, strokeBrush.R, strokeBrush.G, strokeBrush.B };
                            MonitorSettings.MonitorViewSettings.MemoryView.FillBrush = new() { fillBrush.A, fillBrush.R, fillBrush.G, fillBrush.B };
                            break;
                        case HardwareType.GpuNvidia:
                        case HardwareType.GpuAmd:
                            MonitorSettings.IsGpuEnabled = item.IsEnabled;
                            MonitorSettings.MonitorViewSettings.GpuView.ShowLine = item.ShowLine;
                            MonitorSettings.MonitorViewSettings.GpuView.DotDensity = item.DotDensity;
                            MonitorSettings.MonitorViewSettings.GpuView.CanvasHeight = item.CanvasHeight;
                            MonitorSettings.MonitorViewSettings.GpuView.CanvasWidth = item.CanvasWidth;
                            MonitorSettings.MonitorViewSettings.GpuView.Background = new() { background.A, background.R, background.G, background.B };
                            MonitorSettings.MonitorViewSettings.GpuView.Foreground = new() { foreground.A, foreground.R, foreground.G, foreground.B };
                            MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush = new() { strokeBrush.A, strokeBrush.R, strokeBrush.G, strokeBrush.B };
                            MonitorSettings.MonitorViewSettings.GpuView.FillBrush = new() { fillBrush.A, fillBrush.R, fillBrush.G, fillBrush.B };
                            break;
                        case HardwareType.Storage:
                            MonitorSettings.IsStorageEnabled = item.IsEnabled;
                            MonitorSettings.MonitorViewSettings.StorageView.ShowLine = item.ShowLine;
                            MonitorSettings.MonitorViewSettings.StorageView.DotDensity = item.DotDensity;
                            MonitorSettings.MonitorViewSettings.StorageView.CanvasHeight = item.CanvasHeight;
                            MonitorSettings.MonitorViewSettings.StorageView.CanvasWidth = item.CanvasWidth;
                            MonitorSettings.MonitorViewSettings.StorageView.Background = new() { background.A, background.R, background.G, background.B };
                            MonitorSettings.MonitorViewSettings.StorageView.Foreground = new() { foreground.A, foreground.R, foreground.G, foreground.B };
                            MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush = new() { strokeBrush.A, strokeBrush.R, strokeBrush.G, strokeBrush.B };
                            MonitorSettings.MonitorViewSettings.StorageView.FillBrush = new() { fillBrush.A, fillBrush.R, fillBrush.G, fillBrush.B };
                            break;
                        case HardwareType.Network:
                            MonitorSettings.IsNetworkEnabled = item.IsEnabled;
                            MonitorSettings.MonitorViewSettings.NetworkView.ShowLine = item.ShowLine;
                            MonitorSettings.MonitorViewSettings.NetworkView.DotDensity = item.DotDensity;
                            MonitorSettings.MonitorViewSettings.NetworkView.CanvasHeight = item.CanvasHeight;
                            MonitorSettings.MonitorViewSettings.NetworkView.CanvasWidth = item.CanvasWidth;
                            MonitorSettings.MonitorViewSettings.NetworkView.Background = new() { background.A, background.R, background.G, background.B };
                            MonitorSettings.MonitorViewSettings.NetworkView.Foreground = new() { foreground.A, foreground.R, foreground.G, foreground.B };
                            MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush = new() { strokeBrush.A, strokeBrush.R, strokeBrush.G, strokeBrush.B };
                            MonitorSettings.MonitorViewSettings.NetworkView.FillBrush = new() { fillBrush.A, fillBrush.R, fillBrush.G, fillBrush.B };
                            break;
                    }
                });

                HardwareServices = new(MonitorSettings);
                MonitorSettings.Save2Json();
                //Task.Delay(1000).Wait();
                JobManager.AddJob(HardwareServices, e => e.ToRunEvery(MonitorSettings.LoopInterval).Milliseconds().DelayFor(1000));
                JobManager.Start();
                //}).RunInBackground();
            }).ContinueWith(_ =>
            {
                Task.Delay(1000).Wait();
                new Action(() =>
                {
                    IsSaving = Visibility.Collapsed;
                    Opacity = 1d;
                }).RunInBackground();
            });
        }

        private void Cancel()
        {
            CloseDialog("false");
        }

        private void Restore()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
                using Stream stream = assembly.GetManifestResourceStream("SystemMonitor.AppSettings.default.json");
                using StreamReader reader = new(stream);
                string result = reader.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    MonitorSettings = JsonConvert.DeserializeObject<MonitorSettings>(result);
                    MonitorSettings.Configuration = Configuration;
                    InitModel();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion Save & Cancel & Restore

        private void SelectColor(string obj)
        {
            Debug.WriteLine(obj);
            IDialogParameters parameters = new DialogParameters();
            if (obj is not null && obj.Trim() != "")
            {
                SolidColorBrush brush = default;
                switch (obj)
                {
                    case "SelectedSettings.StrokeBrush":
                        {
                            brush = SelectedSettings.StrokeBrush;
                            break;
                        }
                    case "SelectedSettings.FillBrush":
                        {
                            brush = SelectedSettings.FillBrush;
                            break;
                        }
                    case "SelectedSettings.Background":
                        {
                            brush = SelectedSettings.Background;
                            break;
                        }
                    case "SelectedSettings.Foreground":
                        {
                            brush = SelectedSettings.Foreground;
                            break;
                        }
                }
                //parameters.Add(nameof(SolidColorBrush), obj);
                parameters.Add(nameof(SolidColorBrush), brush);
            }

            DialogService.ShowDialog(nameof(Views.ColorPicker), parameters, r =>
            {
                Debug.WriteLine(r.ToString());
                if (r.Result is ButtonResult.OK
                    && r.Parameters?.Count > 0
                    && r.Parameters.ContainsKey("SelectedBrush"))
                {
                    SolidColorBrush selectedBrush = r.Parameters.GetValue<SolidColorBrush>("SelectedBrush");
                    switch (obj)
                    {
                        case "SelectedSettings.StrokeBrush":
                            {
                                SelectedSettings.StrokeBrush = selectedBrush;
                                break;
                            }
                        case "SelectedSettings.FillBrush":
                            {
                                SelectedSettings.FillBrush = selectedBrush;
                                break;
                            }
                        case "SelectedSettings.Background":
                            {
                                SelectedSettings.Background = selectedBrush;
                                break;
                            }
                        case "SelectedSettings.Foreground":
                            {
                                SelectedSettings.Foreground = selectedBrush;
                                break;
                            }
                    }
                }
            });
        }

        private void SelectionChanged(SettingsModels obj)
        {
            if (obj is not null)
            {
                SelectedSettings = obj;
            }
        }

        private void IndexDownChanged(SettingsModels obj)
        {
            if (obj is not null)
            {
                SelectedSettings = obj;
                int index = SettingsModelList.IndexOf(obj);
                SettingsModelList.RemoveAt(index);
                SettingsModelList.Insert(index + 1 < SettingsModelList.Count
                    ? index + 1
                    : SettingsModelList.Count,
                    obj);
            }
        }

        private void IndexUpChanged(SettingsModels obj)
        {
            if (obj is not null)
            {
                SelectedSettings = obj;
                int index = SettingsModelList.IndexOf(obj);
                SettingsModelList.RemoveAt(index);
                SettingsModelList.Insert(index - 1 < 0
                    ? 0
                    : index - 1,
                    obj);
            }
        }

        private static string HardType2String(HardwareType type)
        {
#pragma warning disable IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
            switch (type)
#pragma warning restore IDE0066, IDE0010 // 将 switch 语句转换为表达式 / 补充缺失项
            {
                case HardwareType.Cpu:
                    return "CPU";
                case HardwareType.Memory:
                    return "内存";
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                    return "显卡";
                case HardwareType.Storage:
                    return "存储";
                case HardwareType.Network:
                    return "网络";
                default: return type.ToString("G");
            }
        }

        #region 关闭弹窗

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            switch (parameter?.ToLower(CultureInfo.CurrentCulture))
            {
                case "true":
                    result = ButtonResult.OK;
                    break;
                case "false":
                    result = ButtonResult.Cancel;
                    break;
            }

            RaiseRequestClose(new DialogResult(result));
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

#pragma warning disable IDE0022 // 使用方法的程序块主体
        public bool CanCloseDialog() => true;
#pragma warning restore IDE0022 // 使用方法的程序块主体

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //Message = parameters.GetValue<string>("message");
        }

        #endregion 关闭弹窗
    }
}