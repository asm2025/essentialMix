using System.Collections.ObjectModel;
using System.Windows.Input;
using essentialMix;
using essentialMix.Core.WPF.Commands;
using essentialMix.Patterns.NotifyChange;
using JetBrains.Annotations;

namespace TestWPF.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		private ViewModelBase _selectedViewModel;

		public MainViewModel()
		{
			AppInfo appInfo = new AppInfo(typeof(MainViewModel).Assembly);
			Title = appInfo.Title;
			ViewModels = new ObservableCollection<ViewModelBase>
			{
				new ObservableDictionaryViewModel(),
				new ObservableHashSetViewModel(),
				new ObservableKeyedDictionaryViewModel(),
				new ObservableListViewModel(),
				new ObservableSortedSetViewModel()
			};
			ChangeView = new RelayCommand<ViewModelBase>(vm => SelectedViewModel = vm, vm => vm != SelectedViewModel);
		}

		[NotNull]
		public string Title { get; }

		public ViewModelBase SelectedViewModel
		{
			get => _selectedViewModel;
			set 
			{
				if (_selectedViewModel == value) return;
				_selectedViewModel = value;
				OnPropertyChanged();
			}
		}

		[NotNull]
		public ObservableCollection<ViewModelBase> ViewModels { get; }

		[NotNull]
		public ICommand ChangeView { get; }
	}
}
