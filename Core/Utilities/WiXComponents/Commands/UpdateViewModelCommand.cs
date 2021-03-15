using System;
using System.Collections.Concurrent;
using System.Windows.Input;
using essentialMix.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using WiXComponents.State.Navigators;
using WiXComponents.ViewModels;

namespace WiXComponents.Commands
{
	public class UpdateViewModelCommand : ICommand
	{
		private readonly ConcurrentDictionary<ViewType, ViewModelBase> _viewModels = new ConcurrentDictionary<ViewType, ViewModelBase>();

		[NotNull]
		private readonly INavigator _navigator;

		public UpdateViewModelCommand([NotNull] INavigator navigator)
		{
			_navigator = navigator;
		}

		/// <inheritdoc />
		public event EventHandler CanExecuteChanged;

		/// <inheritdoc />
		public bool CanExecute(object parameter)
		{
			return _navigator.ViewModel == null || !_navigator.ViewModel.IsBusy;
		}

		/// <inheritdoc />
		public void Execute(object parameter)
		{
			ViewType type = GetViewType(parameter);
			IServiceProvider services = App.Instance.ServiceProvider;
			Type viewType = type switch
			{
				ViewType.Home => typeof(HomeViewModel),
				ViewType.Generate => typeof(GenerateViewModel),
				ViewType.Find => typeof(FindViewModel),
				ViewType.About => typeof(AboutViewModel),
				_ => throw new ArgumentOutOfRangeException()
			};
			ILogger logger = (ILogger)services?.GetService(typeof(ILogger<>).MakeGenericType(viewType));
			ViewModelBase viewModel = _viewModels.GetOrAdd(type, _ => (ViewModelBase)viewType.CreateInstance(logger));
			_navigator.ViewModel = viewModel;
		}

		protected void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		protected ViewType GetViewType(object parameter)
		{
			return !(parameter is ViewType viewType)
						? ViewType.Home
						: viewType;
		}
	}
}
