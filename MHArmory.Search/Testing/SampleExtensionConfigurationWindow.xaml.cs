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
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Testing
{
    /// <summary>
    /// Interaction logic for SampleExtensionConfigurationWindow.xaml
    /// </summary>
    public partial class SampleExtensionConfigurationWindow : Window
    {
        private readonly ISolver solver;

        private readonly SampleViewModel viewModel;

        public SampleExtensionConfigurationWindow(ISolver solver)
        {
            InitializeComponent();

            this.solver = solver;

            viewModel = new SampleViewModel(solver);
            DataContext = viewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            viewModel.OnClose();
        }
    }
}
