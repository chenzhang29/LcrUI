using System;
using System.Globalization;
using System.Windows.Data;
using LcrUI.Models;

namespace LcrUI.Converters
{
    public class DisplayPresetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is Preset))
                return string.Empty;

            var p = (Preset)value;
            return $"{p.NumPlayers} players X {p.NumGames}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
