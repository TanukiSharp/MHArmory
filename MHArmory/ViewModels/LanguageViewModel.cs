using MHArmory.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class LanguageViewModel : ViewModelBase
    {
        private readonly string code;

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            private set { SetValue(ref isChecked, value); }
        }

        public string Text { get; }
        public ICommand SelectionCommand { get; }

        public LanguageViewModel(string code, string text)
        {
            this.code = code;
            Text = text;

            SelectionCommand = new AnonymousCommand(OnSelection);
            UpdateIsChecked();

            Localization.LanguageChanged += Localization_LanguageChanged;
        }

        private void Localization_LanguageChanged(object sender, EventArgs e)
        {
            UpdateIsChecked();
        }

        private void UpdateIsChecked()
        {
            IsChecked = (Localization.Language ?? Localization.DefaultLanguage) == code;
        }

        private void OnSelection()
        {
            Localization.Language = code;
        }
    }
}
