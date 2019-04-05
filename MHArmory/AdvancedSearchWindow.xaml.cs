using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MHArmory.Core.DataStructures;
using MHArmory.Search;
using MHArmory.ViewModels;
using MHArmory.Core.WPF;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for AdvancedSearchWindow.xaml
    /// </summary>
    public partial class AdvancedSearchWindow : Window
    {
        private readonly RootViewModel root;

        private bool isOpened;

        public AdvancedSearchWindow(RootViewModel root)
        {
            InitializeComponent();

            this.root = root;

            InputBindings.Add(new InputBinding(new AnonymousCommand(Close), new KeyGesture(Key.Escape, ModifierKeys.None)));

            DataContext = root.AdvancedSearchViewModel;
        }

        public new void Show()
        {
            if (isOpened)
            {
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;
            }

            isOpened = true;

            base.Show();
            Activate();
        }

        public void Update()
        {
            if (isOpened)
                root.UpdateAdvancedSearch();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;

            isOpened = false;

            Hide();
        }
    }
}
