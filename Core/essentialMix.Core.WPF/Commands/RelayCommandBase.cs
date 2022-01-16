using System;
using System.Collections.Generic;
using System.Windows.Input;
using essentialMix.Core.WPF.Helpers;

namespace essentialMix.Core.WPF.Commands;

public abstract class RelayCommandBase : IRelayCommandBase
{
	private bool _isAutomaticRequeryDisabled;
	private IList<WeakReference> _canExecuteChangedHandlers;

	protected RelayCommandBase()
	{
		_canExecuteChangedHandlers = new List<WeakReference>();
	}

	public event EventHandler CanExecuteChanged
	{
		add
		{
			if (!IsAutomaticRequeryDisabled) CommandManager.RequerySuggested += value;
			CommandManagerHelper.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
		}
		remove
		{
			if (!IsAutomaticRequeryDisabled) CommandManager.RequerySuggested -= value;
			CommandManagerHelper.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
		}
	}

	public bool IsAutomaticRequeryDisabled
	{
		get => _isAutomaticRequeryDisabled;
		set
		{
			if (_isAutomaticRequeryDisabled == value) return;

			if (value)
				CommandManagerHelper.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);
			else
				CommandManagerHelper.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);

			_isAutomaticRequeryDisabled = value;
		}
	}

	public void RaiseCanExecuteChanged() { OnCanExecuteChanged(); }

	protected virtual void OnCanExecuteChanged()
	{
		CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
	}
}