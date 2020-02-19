using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MHArmory.Search.Default
{
    public partial class SolverDataConfigurationWindow : Window
    {
        private readonly SolverDataViewModel viewModel;

        public SolverDataConfigurationWindow(SolverData solver)
        {
            InitializeComponent();
            viewModel = new SolverDataViewModel(solver);
            DataContext = viewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            viewModel.OnClose();
        }
    }
}
