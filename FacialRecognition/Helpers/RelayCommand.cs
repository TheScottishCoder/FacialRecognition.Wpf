using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FacialRecognition.Helper
{
    public class RelayCommand : ICommand
    {

        private readonly Predicate<object> _canExecute;
        private readonly Action _execute;

        /// <summary>
        /// Initialise new object of class
        /// </summary>
        /// <param name="execute"></param>
        /// <param name="canExecute"></param>
        public RelayCommand(Action execute, Predicate<object> canExecute = null)
        {
            _canExecute = canExecute ?? (_ => true);
            _execute = execute;
        }

        /// <summary>
        /// Event triggers when changes affect command should execute 
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Checks if the command can execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>true if the command can be executed, otherwise false</returns>
        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute.Invoke(parameter);

        /// <summary>
        /// Executes the commands instance
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object? parameter) =>
            _execute();
    }
}
