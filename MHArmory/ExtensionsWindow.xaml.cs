using MHArmory.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for ExtensionsWindow.xaml
    /// </summary>
    public partial class ExtensionsWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public ExtensionsWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.Extensions;

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));
        }

        private void OnCancel(object parameter)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }
    }
}
