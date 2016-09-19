﻿using System.Windows;

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
