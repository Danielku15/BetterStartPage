using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BetterStartPage.Converter
{
    internal class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool) value)
                {
                    return Invert ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return Invert ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Invert ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                switch ((Visibility)value)
                {
                    case Visibility.Visible:
                        return !Invert;
                    case Visibility.Hidden:
                    case Visibility.Collapsed:
                        return Invert;
                }
            }
            return false;
        }
    }
}
