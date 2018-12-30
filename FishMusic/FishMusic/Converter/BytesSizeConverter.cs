using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FishMusic.Converter
{
    public class BytesSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            var size = (long)value;
            var kb = size / 1024.0;
            if (kb < 1000)
            {
                return Math.Round(kb, 0) + "K";
            }

            var mb = kb / 1024.0;
            return Math.Round(mb, 1) + "M";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}