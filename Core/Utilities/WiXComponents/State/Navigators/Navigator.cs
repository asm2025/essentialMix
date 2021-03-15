using System.Windows.Input;
using essentialMix.Patterns.NotifyChange;
using WiXComponents.Commands;
using WiXComponents.ViewModels;

namespace WiXComponents.State.Navigators
{
	public class Navigator : NotifyPropertyChangedBase, INavigator
	{
		private ViewModelBase _viewModel;

		public Navigator()
		{
			UpdateViewModel = new UpdateViewModelCommand(this);
		}

		/// <inheritdoc />
		public ViewModelBase ViewModel
		{
			get => _viewModel;
			set
			{
				if (_viewModel == value) return;
				_viewModel = value;
				OnPropertyChanged();
			}
		}

		/// <inheritdoc />
		public ICommand UpdateViewModel { get; }
	}
}