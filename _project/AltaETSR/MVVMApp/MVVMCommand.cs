using System;
using System.Windows.Input;

namespace MVVMApp {
    public class MVVMCommand : ICommand {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;


        public MVVMCommand(Action action) {
            _action = action;
        }

        public void Execute(object parameter) {
            _action();
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
