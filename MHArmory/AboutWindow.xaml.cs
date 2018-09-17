using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            txtVersion.Text = App.Version.ToString();
        }

        private void DeveloperInfoButtonClick(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            App.GetAssemblyInfo(sb);

            MessageBox.Show(sb.ToString(), $"Developer information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LinkButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(((FrameworkElement)sender).Tag as string));
        }
    }
}
