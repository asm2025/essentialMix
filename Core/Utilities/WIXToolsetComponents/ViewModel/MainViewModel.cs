using System.Windows.Input;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using WIXToolsetComponents.Services;

namespace WIXToolsetComponents.ViewModel
{
	public sealed class MainViewModel : BindableBase
	{
		private readonly DelegateCommand _generateCommand;
		private readonly DelegateCommand _findMissingCommand;
		private readonly DelegateCommand _cancelCommand;

		[NotNull]
		private readonly IInteractionService _interactionService;

		private string _title;
		private bool _canGenerate;
		private bool _canFindMissing;
		private bool _isCancellationRequested;
		private string _currentAction;
		private string _currentItem;
		private int _progress;

		/// <inheritdoc />
		public MainViewModel([NotNull] IInteractionService interactionService)
		{
			_interactionService = interactionService;
			_generateCommand = new DelegateCommand()
		}

		public string Title
		{
			get => _title;
			set => SetProperty(ref _title, value);
		}

		[NotNull]
		public ICommand GenerateCommand => _generateCommand;

		[NotNull]
		public ICommand FindMissingCommand => _findMissingCommand;

		[NotNull]
		public ICommand CancelCommand => _cancelCommand;

		public bool CanGenerate
		{
			get => _canGenerate;
			set
			{
				SetProperty(ref _canGenerate, value);
				_generateCommand.RaiseCanExecuteChanged();
			}
		}

		public bool CanFindMissing
		{
			get => _canFindMissing;
			set
			{
				SetProperty(ref _canFindMissing, value);
				_findMissingCommand.RaiseCanExecuteChanged();
			}
		}

		public bool IsCancellationRequested
		{
			get => _isCancellationRequested;
			set => SetProperty(ref _isCancellationRequested, value);
		}

		public string CurrentAction
		{
			get => _currentAction;
			set => SetProperty(ref _currentAction, value);
		}

		public string CurrentItem
		{
			get => _currentItem;
			set => SetProperty(ref _currentItem, value);
		}

		public int Progress
		{
			get => _progress;
			set => SetProperty(ref _progress, value);
		}
	}
}
