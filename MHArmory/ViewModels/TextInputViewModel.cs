using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHArmory.Core.WPF;

namespace MHArmory.ViewModels
{
    public class TextInputViewModel : ViewModelBase
    {
        private readonly bool isMandatory;
        private readonly Action<bool> endFunc;

        public string Prompt { get; }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (SetValue(ref text, value) && isMandatory)
                    AcceptCommand.IsEnabled = string.IsNullOrWhiteSpace(text) == false;
            }
        }

        public AnonymousCommand AcceptCommand { get; }
        public ICommand CancelCommand { get; }

        public TextInputViewModel(string prompt, string defaultText, bool isMandatory, Action<bool> endFunc)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                prompt = "Input text:";

            Prompt = prompt;

            this.isMandatory = isMandatory;
            this.endFunc = endFunc;

            AcceptCommand = new AnonymousCommand(OnAccept);
            CancelCommand = new AnonymousCommand(OnCancel);

            Text = defaultText ?? string.Empty;
        }

        private void OnAccept(object parameter)
        {
            endFunc(true);
        }

        private void OnCancel(object parameter)
        {
            endFunc(false);
        }
    }
}
