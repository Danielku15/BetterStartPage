using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BetterStartPage.Control.ViewModel;

namespace BetterStartPage.Converter
{
    public class ProjectDisplayModeToStyleConverter : IValueConverter
    {
        public Style LargeStyle { get; set; }
        public Style SmallStyle { get; set; }
        public Style MiniStyle { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectDisplayModes mode)
            {
                switch (mode)
                {
                    case ProjectDisplayModes.Large:
                        return LargeStyle;
                    case ProjectDisplayModes.Small:
                        return SmallStyle;
                    case ProjectDisplayModes.Mini:
                        return MiniStyle;
                    default:
                        return LargeStyle;
                }
            }

            return LargeStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}