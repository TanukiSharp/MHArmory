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

        private static SearchResultProcessingWindow firstInstance;

        public SearchResultProcessingWindow(RootViewModel rootViewModel)
        {
            if (firstInstance != null)
                throw new InvalidOperationException($"Window {nameof(SearchResultProcessingWindow)} already created.");

            if (firstInstance == null)
                firstInstance = this;

            InitializeComponent();


            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.SearchResultProcessing;
        }

        public static bool Open(Func<SearchResultProcessingWindow> factory)
        {
            if (firstInstance == null)
                firstInstance = factory();

            if (firstInstance.WindowState == WindowState.Minimized)
                firstInstance.WindowState = WindowState.Normal;

            firstInstance.Show();
            firstInstance.Activate();

            return true;
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
