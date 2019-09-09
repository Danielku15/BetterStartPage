using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BetterStartPage.View
{
    class DropDownButtonMenuBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(DropDownButtonMenuBehavior), new PropertyMetadata(default(bool), OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button b)
            {
                if ((bool)e.NewValue)
                {
                    b.Click += ShowContextMenu;
                }
                else
                {
                    b.Click -= ShowContextMenu;
                }
            }
        }

        private static void ShowContextMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.ContextMenu != null)
            {
                b.ContextMenu.IsEnabled = true;
                b.ContextMenu.PlacementTarget = b;
                b.ContextMenu.Placement = PlacementMode.Bottom;
                b.ContextMenu.IsOpen = true;
            }
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        // ReSharper disable once UnusedMember.Global
        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }
    }
}
