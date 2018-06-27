using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MHArmory
{
    /// <summary>
    /// An ICommand that maps to delegate methods.
    /// </summary>
    public class AnonymousCommand : AnonymousCommand<object>
    {
        /// <summary>
        /// Initializes the AnonymousCommand instance.
        /// </summary>
        /// <param name="execute">The delegate method to be executed when command is executed.</param>
        public AnonymousCommand(Action execute)
            : base(p => execute())
        {
        }

        /// <summary>
        /// Initializes the AnonymousCommand instance.
        /// </summary>
        /// <param name="execute">The delegate method to be executed when command is executed.</param>
        public AnonymousCommand(Action<object> execute)
            : base(execute)
        {
        }

        /// <summary>
        /// Initializes the AnonymousCommand instance.
        /// </summary>
        /// <param name="execute">The delegate method to be executed when command is executed.</param>
        /// <param name="canExecute">The delegate method to be executed to check for execution validity.</param>
        public AnonymousCommand(Action<object> execute, Predicate<object> canExecute)
            : base(execute, canExecute)
        {
        }
    }

    /// <summary>
    /// A typed ICommand that maps to delegate methods.
    /// </summary>
    public class AnonymousCommand<T> : ICommand
    {
        private Action<T> execute;
        private Predicate<T> canExecute;

        /// <summary>
        /// Initializes the AnonymousCommand instance.
        /// </summary>
        /// <param name="execute">The delegate method to be executed when command is executed.</param>
        public AnonymousCommand(Action<T> execute)
            : this(execute, p => true)
        {
        }

        /// <summary>
        /// Initializes the AnonymousCommand instance.
        /// </summary>
        /// <param name="execute">The delegate method to be executed when command is executed.</param>
        /// <param name="canExecute">The delegate method to be executed to check for execution validity.</param>
        public AnonymousCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            if (canExecute == null)
                throw new ArgumentNullException("canExecute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        private bool isEnabled = true;

        /// <summary>
        /// Gets or sets whether the command execution is available or not.
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// Checks if command can be executed or not.
        /// </summary>
        /// <param name="parameter">The custom parameter to check execution upon.</param>
        /// <returns>Returns true if the command can be executed, false otherwise.</returns>
        public bool CanExecute(object parameter)
        {
            if (IsEnabled == false)
                return false;

            return canExecute((T)parameter);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The custom parameter to provide to the command at execution time.</param>
        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        /// <summary>
        /// Raised when command execution availability changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
