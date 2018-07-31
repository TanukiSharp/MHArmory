using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MHArmory.ViewModels;

namespace MHArmory
{
    /// <summary>
    /// Interaction logic for TextInputWindow.xaml
    /// </summary>
    public partial class TextInputWindow : Window
    {
        private readonly TextInputViewModel textInputViewModel;

        public string Text { get; private set; }

        public TextInputWindow(string title, string prompt, string defaultText, bool isMandatory)
        {
            InitializeComponent();

            Title = title;
            textInputViewModel = new TextInputViewModel(prompt, defaultText, isMandatory, OnEnd);

            DataContext = textInputViewModel;
        }

        private void OnEnd(bool result)
        {
            if (result)
                Text = textInputViewModel.Text;

            DialogResult = result;
        }
    }
}
