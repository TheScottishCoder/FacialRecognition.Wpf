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

        public RelayCommand(Action execute, Predicate<object> canExecute = null)
        {
            _canExecute = canExecute ?? (_ => true);
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) =>
            _canExecute == null || _canExecute.Invoke(parameter);

        public void Execute(object? parameter) =>
            _execute();
    }
}
