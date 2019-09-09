using System;
using System.Globalization;
using System.Windows.Data;
using BetterStartPage.Control.ViewModel;

namespace BetterStartPage.Converter
{
    public class ProjectDisplayModeToIconSizeConverter : IValueConverter
    {
        public double LargeSize { get; set; }
        public double SmallSize { get; set; }
        public double MiniSize { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectDisplayModes mode)
            {
                switch (mode)
                {
                    case ProjectDisplayModes.Large:
                        return LargeSize;
                    case ProjectDisplayModes.Small:
                        return SmallSize;
                    case ProjectDisplayModes.Mini:
                        return MiniSize;
                    default:
                        return LargeSize;
                }
            }

            return LargeSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}