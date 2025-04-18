using System;
using System.Globalization;
using System.Windows.Data;

namespace Vortex
{
    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString("X8");
            }
            return "00000000";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? hex = value as string;
            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            return 0; // または Binding.DoNothing
        }
    }
}