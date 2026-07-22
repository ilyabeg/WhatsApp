using System.Windows.Input;

namespace WhatsAppUI.View.Helpers
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// If the provided execute action is null throw exceotion (Force a valid Action)
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // if _canExecute is null, return true (meaning the Exectue action is allowed to be executed)
        // else, return _canExecute answear -> (true/false)
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        /// <summary>
        /// 
        /// Gemini explanation:
        /// 
        /// Wires this command into WPF's global event system. 
        /// Whenever the user interacts with the UI (e.g., typing in a textbox, clicking, changing focus),
        /// WPF automatically forces the button to re-run the CanExecute method 
        /// to instantly update its enabled/disabled visual state.
        /// 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
