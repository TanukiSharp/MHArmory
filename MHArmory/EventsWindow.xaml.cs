using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for EventsWindow.xaml
    /// </summary>
    public partial class EventsWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public EventsWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.Events;
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

            rootViewModel.Events.UpdateAndSaveConfiguration();
        }
    }
}
