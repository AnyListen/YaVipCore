using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FishMusic.Converter
{
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return Visibility.Collapsed;
            }

            if (Boolean.Parse(parameter.ToString()))
            {
                return ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                return ((DateTime)value).ToString("yyyy-MM-dd");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}