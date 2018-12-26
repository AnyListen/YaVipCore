using System;
using System.Globalization;
using System.Windows.Data;

namespace FishMusic.Converter
{
    public class UrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            if (value.ToString().StartsWith("http"))
            {
                return value;
            }
            return AnyListen.AnyListen.GetRealUrl(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}