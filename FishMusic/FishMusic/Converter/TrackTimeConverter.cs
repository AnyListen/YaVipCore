using System;
using System.Globalization;
using System.Windows.Data;
using FishMusic.Helper;

namespace FishMusic.Converter
{
    public class TrackTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "00:00:00";
            }
            return double.TryParse(value.ToString(), out var result) ? CommonHelper.SecondsToTime((int) result) : "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}