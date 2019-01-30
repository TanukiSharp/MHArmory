using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MHArmory.ViewModels;
using MHArmory.Core.WPF;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for SkillSelectorWindow.xaml
    /// </summary>
    public partial class SkillSelectorWindow : Window, INotifyPropertyChanged
    {
        public SkillSelectorViewModel SkillSelector { get; } = new SkillSelectorViewModel();

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

        public SkillSelectorWindow()
        {
            InitializeComponent();

            DataContext = this;

            Loaded += SkillSelectorWindow_Loaded;
        }

        private async void SkillSelectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.Yield(DispatcherPriority.SystemIdle);

            //IList<SkillViewModel> skills = await GlobalData.Instance.GetSkills();
            //SkillSelector.Skills = skills;

            InputBindings.Add(new InputBinding(new AnonymousCommand(OnCancel), new KeyGesture(Key.Escape, ModifierKeys.None)));

            IsDataLoading = false;
            IsDataLoaded = true;
        }

        private void OnCancel(object parameter)
        {
            var cancellable = new CancellationCommandArgument();

            SkillSelector.CancelCommand.ExecuteIfPossible(cancellable);

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        #endregion
    }
}
