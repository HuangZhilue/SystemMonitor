using LibreHardwareMonitor.Hardware;
using Prism.Mvvm;
using System.Windows.Media;

namespace SystemMonitor.Models.SettingsModel
{
    public class SettingsModels : BindableBase
    {
        private int hardwareIndex;
        private HardwareType hardwareType;
        private string hardwareTypeString;
        private bool isEnabled;
        private int dotDensity = 60;
        private int canvasHeight = 55;
        private int canvasWidth = 60;
        private bool showLine = true;
        private double fontSize = 12d;
        private SolidColorBrush strokeBrush = Brushes.Green;
        private SolidColorBrush fillBrush = Brushes.Green;
        private SolidColorBrush background = Brushes.Black;
        private SolidColorBrush foreground = Brushes.Green;

        public int HardwareIndex { get => hardwareIndex; set => SetProperty(ref hardwareIndex, value); }
        public HardwareType HardwareType { get => hardwareType; set => SetProperty(ref hardwareType, value); }
        public string HardwareTypeString { get => hardwareTypeString; set => SetProperty(ref hardwareTypeString, value); }
        public bool IsEnabled { get => isEnabled; set => SetProperty(ref isEnabled, value); }
        public int DotDensity { get => dotDensity; set => SetProperty(ref dotDensity, value); }
        public int CanvasHeight { get => canvasHeight; set => SetProperty(ref canvasHeight, value); }
        public int CanvasWidth { get => canvasWidth; set => SetProperty(ref canvasWidth, value); }
        public bool ShowLine { get => showLine; set => SetProperty(ref showLine, value); }
        public double FontSize { get => fontSize; set => SetProperty(ref fontSize, value); }
        public SolidColorBrush StrokeBrush { get => strokeBrush; set => SetProperty(ref strokeBrush, value); }
        public SolidColorBrush FillBrush { get => fillBrush; set => SetProperty(ref fillBrush, value); }
        public SolidColorBrush Background { get => background; set => SetProperty(ref background, value); }
        public SolidColorBrush Foreground { get => foreground; set => SetProperty(ref foreground, value); }
    }
}