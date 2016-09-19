using System;
using System.Globalization;
using System.Windows.Data;

namespace BetterStartPage.Control.Converter
{
    internal class IntegerValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int numeric;
            if (value == null)
            {
                return 0;
            }
            if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out numeric))
            {
                return numeric;
            }
            return 0;
        }
    }
}
