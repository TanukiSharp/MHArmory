using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MHArmory.Services
{
    public class RenameOptions
    {
        public string WindowTitle;
        public string WindowPrompt;
        public string WindowDefaultValue;
        public bool IsInputMandatory;
        public Func<string, bool> IsValid;
    }

    public interface IRenameService
    {
        bool Rename(RenameOptions renameOptions, out string newName);
    }

    public class RenameService : IRenameService
    {
        public bool Rename(RenameOptions renameOptions, out string newName)
        {
            if (renameOptions == null)
                throw new ArgumentNullException(nameof(renameOptions));

            newName = null;

            var inputWindow = new TextInputWindow(
                renameOptions.WindowTitle ?? "Rename",
                renameOptions.WindowPrompt ?? "Rename:",
                renameOptions.WindowDefaultValue,
                renameOptions.IsInputMandatory
            );

            if (inputWindow.ShowDialog() != true)
                return false;

            if (inputWindow.Text == renameOptions.WindowDefaultValue)
                return false;

            if (renameOptions.IsValid != null && renameOptions.IsValid(inputWindow.Text) == false)
            {
                MessageBox.Show($"Invalid name '{inputWindow.Text}'.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            newName = inputWindow.Text;
            return true;
        }
    }
}
