using System;
using System.Windows.Input;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Commands;

public class RelayCommand : RelayCommandBase, IRelayCommand
{
	[NotNull]
	private readonly Action<IRelayCommand> _execute;
	private readonly Func<IRelayCommand, bool> _canExecute;

	public RelayCommand([NotNull] Action<IRelayCommand> execute)
		: this(execute, null)
	{
	}

	public RelayCommand([NotNull] Action<IRelayCommand> execute, Func<IRelayCommand, bool> canExecute)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	/// <inheritdoc />
	bool ICommand.CanExecute(object parameter) { return CanExecute(); }

	public bool CanExecute() { return _canExecute == null || _canExecute(this); }

	/// <inheritdoc />
	void ICommand.Execute(object parameter) { Execute(); }
	public void Execute() { _execute(this); }
}

public class RelayCommand<T> : RelayCommandBase, IRelayCommand
{
	[NotNull]
	private readonly Action<IRelayCommand, T> _execute;
	private readonly Func<IRelayCommand, T, bool> _canExecute;

	public RelayCommand([NotNull] Action<IRelayCommand, T> execute)
		: this(execute, null)
	{
	}

	public RelayCommand([NotNull] Action<IRelayCommand, T> execute, Func<IRelayCommand, T, bool> canExecute)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	/// <inheritdoc />
	bool ICommand.CanExecute(object parameter)
	{
		return parameter is null && typeof(T).IsValueType
					? _canExecute == null
					: CanExecute((T)parameter);
	}

	public bool CanExecute(T parameter) { return _canExecute == null || _canExecute(this, parameter); }

	/// <inheritdoc />
	void ICommand.Execute(object parameter) { Execute((T)parameter); }
	public void Execute(T parameter) { _execute(this, parameter); }
}