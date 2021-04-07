using System;
using System.Collections.Generic;
using System.Windows.Input;
using essentialMix.Core.WPF.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Commands
{
	public class RelayCommand : IRelayCommand
	{
		[NotNull]
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;
		private bool _isAutomaticRequeryDisabled;
		private IList<WeakReference> _canExecuteChangedHandlers;

		public RelayCommand([NotNull] Action execute)
			: this(execute, null, false)
		{
		}

		public RelayCommand([NotNull] Action execute, Func<bool> canExecute)
			: this(execute, canExecute, false)
		{
		}

		public RelayCommand([NotNull] Action execute, Func<bool> canExecute, bool isAutomaticRequeryDisabled)
		{
			_execute = execute;
			_canExecute = canExecute;
			_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
			_canExecuteChangedHandlers = new List<WeakReference>();
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

		/// <inheritdoc />
		bool ICommand.CanExecute(object parameter) => CanExecute();
		public bool CanExecute() { return _canExecute == null || _canExecute(); }

		/// <inheritdoc />
		void ICommand.Execute(object parameter) { Execute(); }
		public void Execute() { _execute(); }

		public void RaiseCanExecuteChanged() { OnCanExecuteChanged(); }

		protected virtual void OnCanExecuteChanged()
		{
			CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
		}
	}

	public class RelayCommand<T> : IRelayCommand
	{
		[NotNull]
		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;
		private bool _isAutomaticRequeryDisabled;
		private IList<WeakReference> _canExecuteChangedHandlers;

		public RelayCommand([NotNull] Action<T> execute)
			: this(execute, null, false)
		{
		}

		public RelayCommand([NotNull] Action<T> execute, Predicate<T> canExecute)
			: this(execute, canExecute, false)
		{
		}

		public RelayCommand([NotNull] Action<T> execute, Predicate<T> canExecute, bool isAutomaticRequeryDisabled)
		{
			_execute = execute;
			_canExecute = canExecute;
			_isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
			_canExecuteChangedHandlers = new List<WeakReference>();
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

		/// <inheritdoc />
		bool ICommand.CanExecute(object parameter)
		{
			return parameter is null && typeof(T).IsValueType
						? _canExecute == null
						: CanExecute((T)parameter);
		}

		public bool CanExecute(T parameter) { return _canExecute == null || _canExecute(parameter); }

		/// <inheritdoc />
		void ICommand.Execute(object parameter) { Execute((T)parameter); }
		public void Execute(T parameter) { _execute(parameter); }

		public void RaiseCanExecuteChanged() { OnCanExecuteChanged(); }

		protected virtual void OnCanExecuteChanged()
		{
			CommandManagerHelper.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
		}
	}
}
