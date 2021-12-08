using LibreHardwareMonitor.Hardware;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SystemMonitor.Models
{
    public class DisplayItems : BindableBase
    {
        private Visibility _visibility = Visibility.Collapsed;
        private Visibility _lineVisibility = Visibility.Visible;
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
        private double _item8FontSize;
        private float _pointData;
        private int _maxPointData = 100;
        private Brush _strokeBrush;
        private Brush _fillBrush;
        private Brush _background;
        private Brush _foreground;
        private PointCollection _pointCollection = new();
        private int _canvasHeight;
        private int _dotDensity;
        private int _canvasWidth;
        private double fontSize = 12d;

        public Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        public Visibility LineVisibility
        {
            get => _lineVisibility;
            set => SetProperty(ref _lineVisibility, value);
        }

        public Identifier Identifier { get; set; }

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

        public double Item8FontSize
        {
            get => _item8FontSize;
            set => SetProperty(ref _item8FontSize, value);
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

        public Brush Background
        {
            get => _background;
            set => SetProperty(ref _background, value);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => SetProperty(ref _foreground, value);
        }

        public PointCollection PointCollection
        {
            get => _pointCollection;
            set => SetProperty(ref _pointCollection, value);
        }

        public int CanvasHeight
        {
            get => _canvasHeight;
            set => SetProperty(ref _canvasHeight, value);
        }

        public int DotDensity
        {
            get => _dotDensity < 50 ? 50 : _dotDensity;
            set => SetProperty(ref _dotDensity, value);
        }

        public int CanvasWidth
        {
            get => _canvasWidth;
            set => SetProperty(ref _canvasWidth, value);
        }

        public double FontSize
        {
            get => fontSize;
            set => SetProperty(ref fontSize, value);
        }

        public float LastMaxY { get; set; } = 100f;
        public LimitList<float> NetworkSpeedList { get; } = new(60);

        public void ClearData()
        {
            Name = Index = Item1 = Item2 = Item3 = Item4 = Item5 = Item6 = Item7 = Item8 = null;
            PointData = 0;
            MaxPointData = 100;
        }

        public void CloneBrush()
        {
            Background = Background.Clone();
            Foreground = Foreground.Clone();
            FillBrush = FillBrush.Clone();
            StrokeBrush = StrokeBrush.Clone();
        }

        public float InsertAndMove(int index = 1, int pointNum = 1)
        {
            Point point = new(0, PointData * ((CanvasHeight - 5) / (double)MaxPointData));
            PointCollection.Insert(index, point);
            PointCollection.RemoveAt(DotDensity + 2);
            //Vector v = new(item.CanvasWidth / (double)item.DotDensity, 0);

            float maxY = NetworkSpeedList.OrderByDescending(x => x).Take(10).Average() / 0.8f;
            maxY = maxY < 1 ? 1 : maxY;
            if (Math.Abs(maxY - LastMaxY) > LastMaxY * 0.05f)
            {
                // 相差超过5%时，使用新的“最大值”
                LastMaxY = maxY;
            }
            //Debug.WriteLine($"{lastMaxY}\t\t{item.PointData}");
            for (int i = pointNum; i < PointCollection.Count - pointNum; i++)
            {
                // 0.5 = pointData / MaxY * H
                Point p = PointCollection[i];
                p.Y = NetworkSpeedList[i - pointNum] / LastMaxY * CanvasHeight;
                p.X += CanvasWidth / (double)DotDensity;
                PointCollection[i] = p;
            }

            return LastMaxY;
        }
    }
}