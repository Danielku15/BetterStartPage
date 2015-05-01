using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BetterStartPage.Control
{
    /// <summary>
    /// Interaction logic for ProjectRenameWindow.xaml
    /// </summary>
    public partial class ProjectRenameWindow : Window
    {
        public ProjectRenameWindow()
        {
            InitializeComponent();
        }

        private void OnRenameClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ProjectNameTextBox.Focus();
            ProjectNameTextBox.SelectAll();
        }
    }
}
