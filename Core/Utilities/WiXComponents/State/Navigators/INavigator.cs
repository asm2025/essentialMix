using System.Windows.Input;
using JetBrains.Annotations;
using WiXComponents.ViewModels;

namespace WiXComponents.State.Navigators
{
	public interface INavigator
	{
		public ViewModelBase ViewModel { get; set; }
		
		[NotNull]
		public ICommand UpdateViewModel { get; }
	}
}
