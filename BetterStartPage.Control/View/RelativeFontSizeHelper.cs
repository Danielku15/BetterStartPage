using System;
using System.Windows;
using System.Windows.Controls;

namespace BetterStartPage.Control.View
{
    internal class RelativeFontSizeHelper
    {
        public static readonly DependencyProperty FontSizeScaleProperty = DependencyProperty.RegisterAttached(
            "FontSizeScale", typeof(double), typeof(RelativeFontSizeHelper), new PropertyMetadata(default(double), OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var block = d as TextBlock;
            if (block != null)
            {
                block.FontSize = Math.Max(1, block.FontSize * GetFontSizeScale(d));
                return;
            }

            var control = d as System.Windows.Controls.Control;
            if (control != null)
            {
                control.FontSize = Math.Max(1, control.FontSize * GetFontSizeScale(d));
            }
        }

        public static void SetFontSizeScale(DependencyObject element, double value)
        {
            element.SetValue(FontSizeScaleProperty, value);
        }

        public static double GetFontSizeScale(DependencyObject element)
        {
            return (double) element.GetValue(FontSizeScaleProperty);
        }
    }
}
