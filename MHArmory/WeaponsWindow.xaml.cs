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
using MHArmory.ViewModels;
using MHArmory.Core.WPF;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for WeaponsWindow.xaml
    /// </summary>
    public partial class WeaponsWindow : Window, IWindow
    {
        private readonly RootViewModel rootViewModel;

        public WeaponsWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            this.rootViewModel = rootViewModel;

            this.Loaded += WeaponsWindow_Loaded;
        }

        private async void WeaponsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = rootViewModel.WeaponsContainer;

            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.SystemIdle);

            rootViewModel.WeaponsContainer.ActivateDefaultIfNeeded();
        }

        public void OnOpening(bool isAlreadyOpened, object argument)
        {
            if (isAlreadyOpened)
                return;

            rootViewModel.WeaponsContainer.UpdateHighlights();
        }

        public void OnOpened(bool isAlreadyOpened, object argument)
        {
        }

        private void OnCancel()
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
