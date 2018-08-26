using System;
using System.Collections.Generic;
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

        private static long openedCount;
        public static bool IsOpened => Interlocked.Read(ref openedCount) > 0;

        public SearchResultProcessingWindow(RootViewModel rootViewModel)
        {
            InitializeComponent();

            Interlocked.Increment(ref openedCount);

            this.rootViewModel = rootViewModel;

            DataContext = rootViewModel.SearchResultProcessing;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            rootViewModel.SearchResultProcessing.SaveConfiguration();
            Interlocked.Decrement(ref openedCount);
        }
    }
}
