using System;
using System.Globalization;
using System.Windows.Data;

namespace FishMusic.Converter
{
    public class PrecentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return 0;
            }
            if (float.TryParse(values[0].ToString(), out var precentResult))
            {
                if (float.TryParse(values[1].ToString(), out var totalResult))
                {
                    return (int) (precentResult * totalResult);
                }
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}