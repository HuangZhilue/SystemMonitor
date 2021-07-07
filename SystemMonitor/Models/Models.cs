using Microsoft.Extensions.DependencyInjection;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SystemMonitor.Helper;
using SystemMonitor.Models.SettingsModel;
using LibreHardwareMonitor.Hardware;

namespace SystemMonitor.Models
{
    //public class DataModel
    //{
    //    public SuspendedObservableCollection<DisplayItems> Models { get; set; } = new();

    //    public void PauseObservable()
    //    {
    //        Models.IsBindingPaused = true;
    //    }

    //    public void Observable()
    //    {
    //        Models.IsBindingPaused = false;
    //    }
    //}

    public class DisplayItems : BindableBase
    {
        //private HardwareType _itemType;
        private string _name;
        private string _name2;
        private string _index;
        private string _item1;
        private string _item2;
        private string _item3;
        private string _item4;
        private string _item5;
        private string _item6;
        private string _item7;
        private string _item8;
        private float _pointData;
        private int _maxPointData = 100;
        private Brush _strokeBrush;
        private Brush _fillBrush;
        private PointCollection _pointCollection = new();

        public Identifier Identifier { get; set; }

        //public HardwareType ItemType
        //{
        //    get => _itemType;
        //    set => SetProperty(ref _itemType, value);
        //}

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }        
        
        public string Name2
        {
            get => _name2;
            set => SetProperty(ref _name2, value);
        }

        public string Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public string Item1
        {
            get => _item1;
            set => SetProperty(ref _item1, value);
        }

        public string Item2
        {
            get => _item2;
            set => SetProperty(ref _item2, value);
        }

        public string Item3
        {
            get => _item3;
            set => SetProperty(ref _item3, value);
        }

        public string Item4
        {
            get => _item4;
            set => SetProperty(ref _item4, value);
        }

        public string Item5
        {
            get => _item5;
            set => SetProperty(ref _item5, value);
        }

        public string Item6
        {
            get => _item6;
            set => SetProperty(ref _item6, value);
        }

        public string Item7
        {
            get => _item7;
            set => SetProperty(ref _item7, value);
        }

        public string Item8
        {
            get => _item8;
            set => SetProperty(ref _item8, value);
        }

        public float PointData
        {
            get => _pointData;
            set => SetProperty(ref _pointData, value);
        }

        public int MaxPointData
        {
            get => _maxPointData;
            set => SetProperty(ref _maxPointData, value);
        }

        //public ChartModel ChartModel { get; set; } = new();
        public Brush StrokeBrush
        {
            get => _strokeBrush;
            set => SetProperty(ref _strokeBrush, value);
        }

        public Brush FillBrush
        {
            get => _fillBrush;
            set => SetProperty(ref _fillBrush, value);
        }

        public PointCollection PointCollection
        {
            get => _pointCollection;
            set => SetProperty(ref _pointCollection, value);
        }

        public void ClearData()
        {
            Name = Index = Item1 = Item2 = Item3 = Item4 = Item5 = Item6 = Item7 = Item8 = null;
            PointData = 0;
            MaxPointData = 100;
        }

        public DisplayItems()
        {
            PointCollection.Add(new Point(0, 0));

            for (var i = 0; i <= 60; i++)
            {
                PointCollection.Add(new Point(i, 0));
            }

            PointCollection.Add(new Point(60, 0));
        }
    }

    //public class ModelColor
    //{
    //    protected static MonitorSettings MonitorSettings { get; } =
    //        Di.ServiceProvider.GetRequiredService<MonitorSettings>();

    //    public Brush StrokeBrush { get; set; }
    //    public Brush FillBrush { get; set; }
    //}

    //public class CpuModel : ModelColor
    //{
    //    private CpuModel()
    //    {
    //        StrokeBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[0],
    //                MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[1],
    //                MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[2],
    //                MonitorSettings.MonitorViewSettings.CpuView.StrokeBrush[3]));
    //        FillBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.CpuView.FillBrush[0],
    //                MonitorSettings.MonitorViewSettings.CpuView.FillBrush[1],
    //                MonitorSettings.MonitorViewSettings.CpuView.FillBrush[2],
    //                MonitorSettings.MonitorViewSettings.CpuView.FillBrush[3]));
    //    }

    //    public static CpuModel Model { get; } = new();
    //    public string CpuName { get; set; }
    //    public float CpuClock => CpuClocks.Sum() / CpuClocks.Count;
    //    public ObservableCollection<float> CpuClocks { get; set; } = new();
    //    public float CpuLoad { get; set; }
    //    public float CpuTemperatures { get; set; } = -999;
    //}

    //public class GpuModel : ModelColor
    //{
    //    private GpuModel()
    //    {
    //        StrokeBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[0],
    //                MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[1],
    //                MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[2],
    //                MonitorSettings.MonitorViewSettings.GpuView.StrokeBrush[3]));
    //        FillBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.GpuView.FillBrush[0],
    //                MonitorSettings.MonitorViewSettings.GpuView.FillBrush[1],
    //                MonitorSettings.MonitorViewSettings.GpuView.FillBrush[2],
    //                MonitorSettings.MonitorViewSettings.GpuView.FillBrush[3]));
    //    }

    //    public static GpuModel Model { get; } = new();
    //    public string GpuName { get; set; }
    //    public float GpuClock4Core { get; set; }
    //    public float GpuClock4Memory { get; set; }
    //    public float GpuLoad { get; set; }
    //    public float GpuTemperatures { get; set; } = -999;
    //}

    //public class MemoryModel : ModelColor
    //{
    //    private MemoryModel()
    //    {
    //        StrokeBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[0],
    //                MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[1],
    //                MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[2],
    //                MonitorSettings.MonitorViewSettings.MemoryView.StrokeBrush[3]));
    //        FillBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[0],
    //                MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[1],
    //                MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[2],
    //                MonitorSettings.MonitorViewSettings.MemoryView.FillBrush[3]));
    //    }

    //    public static MemoryModel Model { get; } = new();

    //    public float MemoryLoad { get; set; }

    //    //public float MemoryLoad4Virtual { get; set; }
    //    public float MemoryUsed { get; set; }

    //    //public float MemoryUsed4Virtual { get; set; }
    //    public float MemoryAvailable { get; set; }

    //    //public float MemoryAvailable4Virtual { get; set; }
    //    public float MemoryTotal => MemoryUsed + MemoryAvailable;
    //    public string MemoryUsedDisplay => MemoryUsed.ToString("0.0") + "/" + MemoryTotal.ToString("0.0") + "GB";
    //}

    //public class DiskModel : ModelColor
    //{
    //    private DiskModel()
    //    {
    //        StrokeBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[0],
    //                MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[1],
    //                MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[2],
    //                MonitorSettings.MonitorViewSettings.StorageView.StrokeBrush[3]));
    //        FillBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.StorageView.FillBrush[0],
    //                MonitorSettings.MonitorViewSettings.StorageView.FillBrush[1],
    //                MonitorSettings.MonitorViewSettings.StorageView.FillBrush[2],
    //                MonitorSettings.MonitorViewSettings.StorageView.FillBrush[3]));
    //    }

    //    public static DiskModel Model { get; } = new();
    //    public string DiskName { get; set; }
    //    public string DiskType { get; set; }
    //    public float DiskLoad4UsedSpace { get; set; }
    //    public float DiskLoad4WriteActivity { get; set; }
    //    public float DiskLoad4TotalActivity { get; set; }
    //    public float DiskThroughput4ReadRate { get; set; }
    //    public float DiskThroughput4WriteRate { get; set; }
    //}

    //public class NetworkModel : ModelColor
    //{
    //    private NetworkModel()
    //    {
    //        StrokeBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[0],
    //                MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[1],
    //                MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[2],
    //                MonitorSettings.MonitorViewSettings.NetworkView.StrokeBrush[3]));
    //        FillBrush = new SolidColorBrush(
    //            Color.FromArgb(
    //                MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[0],
    //                MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[1],
    //                MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[2],
    //                MonitorSettings.MonitorViewSettings.NetworkView.FillBrush[3]));
    //    }

    //    public static NetworkModel Model { get; } = new();
    //    public string NetworkName { get; set; }
    //    public bool IsActive { get; set; }
    //    public float NetworkThroughput4UploadSpeed { get; set; }
    //    public float NetworkThroughput4DownloadSpeed { get; set; }
    //    public float NetworkLoad { get; set; }
    //}

    //public class SuspendedObservableCollection<T> : ObservableCollection<T>
    //{
    //    private bool _isBindingPaused;

    //    public bool IsBindingPaused
    //    {
    //        get => _isBindingPaused;
    //        set
    //        {
    //            _isBindingPaused = value;
    //            if (!value)
    //            {
    //                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    //            }
    //        }
    //    }

    //    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    //    {
    //        if (!IsBindingPaused)
    //        {
    //            base.OnCollectionChanged(e);
    //        }
    //    }
    //}
}