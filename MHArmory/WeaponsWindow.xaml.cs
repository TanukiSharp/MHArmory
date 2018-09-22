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

            WindowManager.FitInScreen(this);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            this.rootViewModel = rootViewModel;

            this.Loaded += WeaponsWindow_Loaded;
        }

        private async void WeaponsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);

            WeaponsContainerViewModel container = rootViewModel.WeaponsContainer;

            container.IsDataLoaded = false;
            container.IsDataLoading = true;

            DataContext = rootViewModel.WeaponsContainer;

            container.IsDataLoaded = true;
            container.IsDataLoading = false;
        }

        public void OnOpen(bool isAlreadyOpened)
        {
            if (isAlreadyOpened)
                return;

            IList<int> inputSlots = rootViewModel.InParameters.Slots
                .Select(x => x.Value)
                .Where(x => x > 0)
                .OrderByDescending(x => x)
                .ToList();

            rootViewModel.WeaponsContainer.UpdateHighlights(inputSlots);
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
