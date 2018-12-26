using System;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace FishMusic.Converter
{
    public class PlayStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return PackIconMaterialKind.Play;
            }
            if (bool.TryParse(value.ToString(), out var playResult))
            {
                return playResult ? PackIconMaterialKind.Pause : PackIconMaterialKind.Play;
            }
            return PackIconMaterialKind.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}