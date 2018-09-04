using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for SearchResultProcessingWindow.xaml
    /// </summary>
    public partial class SearchResultProcessingWindow : Window
    {
        private readonly RootViewModel rootViewModel;

        public SearchResultProcessingWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            WindowManager.FitInScreen(this);

            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.SearchResultProcessing;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
            rootViewModel.SearchResultProcessing.SaveConfiguration();
        }
    }
}
