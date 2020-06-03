using MHArmory.Core.WPF;
using MHArmory.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for ArmorSetBonusSelectorWindow.xaml
    /// </summary>
    public partial class ArmorSetBonusSelectorWindow : Window, INotifyPropertyChanged
    {
        public ArmorSetBonusSelectorViewModel ArmorSetSelector { get; } = new ArmorSetBonusSelectorViewModel();

        private bool isDataLoading = true;
        public bool IsDataLoading
        {
            get { return isDataLoading; }
            private set { SetValue(ref isDataLoading, value); }
        }

        private bool isDataLoaded;

        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            private set { SetValue(ref isDataLoaded, value); }
        }

        public ArmorSetBonusSelectorWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += ArmorSetBonusSelectorWindow_Loaded;
        }

        private async void ArmorSetBonusSelectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.SystemIdle);

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            IsDataLoading = false;
            IsDataLoaded = true;
        }

        private void OnCancel(object parameter)
        {
            var cancellable = new CancellationCommandArgument();

            ArmorSetSelector.CancelCommand.ExecuteIfPossible(cancellable);

            if (cancellable.IsCancelled == false)
                Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            Hide();
        }


        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
            {
                field = value;
                PropertyChanged?.Invoke(this, PropertyChangedEventArgsCache.Get(propertyName));
                return true;
            }

            return false;
        }

        #endregion
    }
}
