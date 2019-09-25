using System;
using System.Windows.Input;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Handy ICommand implementation for just calling a delegate
    /// </summary>
    internal class DelegateCommand : PropertyNotifier, ICommand
    {
        public event EventHandler CanExecuteChanged;

        private bool? canExecute;
        private readonly Action<object> executeAction;
        private readonly Func<object, bool> canExecuteFunc;

        public DelegateCommand(Action executeAction, Func<bool> canExecuteFunc = null)
        {
            this.executeAction = (object arg) => executeAction?.Invoke();
            this.canExecuteFunc = (object arg) => canExecuteFunc?.Invoke() ?? true;
        }

        public DelegateCommand(Action<object> executeAction = null, Func<object, bool> canExecuteFunc = null)
        {
            this.executeAction = executeAction;
            this.canExecuteFunc = canExecuteFunc;
        }

        public void UpdateCanExecute()
        {
            this.canExecute = null;
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            this.OnPropertyChanged(nameof(this.CanExecute));
        }

        public bool CanExecute
        {
            get
            {
                return ((ICommand)this).CanExecute(null);
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (this.canExecute == null)
            {
                this.canExecute = this.canExecuteFunc?.Invoke(parameter) ?? true;
            }

            return this.canExecute == true;
        }

        public void Execute(object parameter)
        {
            this.executeAction?.Invoke(parameter);
        }
    }
}
