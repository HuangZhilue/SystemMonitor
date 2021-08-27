using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SystemMonitor.Helper
{
    //public class TimeConverter : IValueConverter
    //{
    //    //当值从绑定源传播给绑定目标时，调用方法Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return DependencyProperty.UnsetValue;
    //        var date = (DateTime)value;
    //        return date.ToString("yyyy-MM-dd");
    //    }

    //    //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var str = value as string;
    //        return DateTime.TryParse(str, out var txtDate) ? txtDate : DependencyProperty.UnsetValue;
    //    }
    //}

    //public class PercentConverter : IValueConverter
    //{
    //    //当值从绑定源传播给绑定目标时，调用方法Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return DependencyProperty.UnsetValue;
    //        var date = System.Convert.ToInt32(value);
    //        return date + "%";
    //    }

    //    //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value != null ? (float)System.Convert.ToInt32(value.ToString()?.TrimEnd('%')) : DependencyProperty.UnsetValue;
    //    }
    //}

    //public class GHzConverter : IValueConverter
    //{
    //    //当值从绑定源传播给绑定目标时，调用方法Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return DependencyProperty.UnsetValue;
    //        var date = (float)value / 1024;
    //        if (date >= 1)
    //        {
    //            return date.ToString("0.00GHz");
    //        }
    //        else if (date < 1)
    //        {
    //            return (date * 1024).ToString("0MHz");
    //        }
    //        else
    //        {
    //            return (date * 1024).ToString("0Hz");
    //        }
    //    }

    //    //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value != null ? (float)System.Convert.ToDouble(value.ToString()?.TrimEnd('G', 'H', 'z')) : DependencyProperty.UnsetValue;
    //    }
    //}

    //public class CelsiusConverter : IValueConverter
    //{
    //    //当值从绑定源传播给绑定目标时，调用方法Convert
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null || value.ToString() == "-999")
    //            return DependencyProperty.UnsetValue;
    //        var date = System.Convert.ToInt32(value);
    //        return date + "℃";
    //    }

    //    //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return value != null ? (float)System.Convert.ToDouble(value.ToString()?.TrimEnd('℃')) : DependencyProperty.UnsetValue;
    //    }
    //}

    public class IsNullOrEmptyConverter : IValueConverter
    {
        /// <summary>
        /// 当值从绑定源传播给绑定目标时，调用方法Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null && !string.IsNullOrWhiteSpace(value.ToString())) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}