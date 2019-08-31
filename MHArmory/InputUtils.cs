using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MHArmory
{
    public class InputOptions
    {
        public string WindowTitle;
        public string WindowPrompt;
        public string WindowDefaultValue;
        public bool IsInputMandatory;
        public Func<string, bool> IsValid;
    }

    public static class InputUtils
    {
        public static bool Show(InputOptions inputOptions, out string newInput)
        {
            if (inputOptions == null)
                throw new ArgumentNullException(nameof(inputOptions));

            newInput = null;

            var inputWindow = new TextInputWindow(
                inputOptions.WindowTitle ?? "Input",
                inputOptions.WindowPrompt ?? "New input:",
                inputOptions.WindowDefaultValue,
                inputOptions.IsInputMandatory
            );

            if (inputWindow.ShowDialog() != true)
                return false;

            if (inputWindow.Text == inputOptions.WindowDefaultValue)
                return false;

            if (inputOptions.IsValid != null && inputOptions.IsValid(inputWindow.Text) == false)
            {
                MessageBox.Show($"Invalid input '{inputWindow.Text}'.", "Invalid input", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            newInput = inputWindow.Text;

            return true;
        }
    }
}
