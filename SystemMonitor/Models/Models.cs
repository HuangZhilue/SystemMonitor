using LibreHardwareMonitor.Hardware;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Media;

namespace SystemMonitor.Models
{
    public class DisplayItems : BindableBase
    {
        private Visibility _visibility = Visibility.Collapsed;
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

        public Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
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
}