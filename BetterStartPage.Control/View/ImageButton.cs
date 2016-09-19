using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetterStartPage.Control.View
{
    internal class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource Image
        {
            get { return (ImageSource) GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
    }
}
