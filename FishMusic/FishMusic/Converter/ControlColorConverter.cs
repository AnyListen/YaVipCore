using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FishMusic.Converter
{
    public class ControlColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Color.FromRgb(70, 70, 70));
            }
            if (bool.TryParse(value.ToString(), out var result))
            {
                return result ? new SolidColorBrush(Color.FromRgb(85, 166, 55)) : new SolidColorBrush(Color.FromRgb(70, 70, 70));
            }
            return new SolidColorBrush(Color.FromRgb(70, 70, 70));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}