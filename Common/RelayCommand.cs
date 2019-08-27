using System;
using System.Diagnostics;
using System.Windows.Input;


namespace PhotoUploader.Common
{
 public class RelayCommand : ICommand
{
    #region Fields

    readonly Action<object> _execute;
    readonly Predicate<object> _canExecute;
    readonly EventHandler _canExcecuteEventHandler;

    #endregion // Fields

    #region Constructors

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action<object> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object> execute, EventHandler canExecute,string empty)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExcecuteEventHandler = canExecute;
    }


    #endregion // Constructors

    #region ICommand Members

    [DebuggerStepThrough]
    public bool CanExecute(object parameters)
    {
        return _canExecute == null ? true : _canExecute(parameters);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameters)
    {
        _execute(parameters);
    }


    public void RaiseCanExecuteChanged()
    {
        if (_canExcecuteEventHandler != null)
            OnCanExecuteChanged();
    }


    protected virtual void OnCanExecuteChanged()
    {
        EventHandler eCanExecuteChanged = _canExcecuteEventHandler;
        if (eCanExecuteChanged != null)
            eCanExecuteChanged(this, EventArgs.Empty);
    }

    #endregion // ICommand Members
}
  
}


