using Microsoft.Extensions.Logging;
using WiXComponents.State.Navigators;

namespace WiXComponents.ViewModels
{
	/// <inheritdoc />
	public class MainViewModel : AppViewModel
	{
		private INavigator _navigator;

		/// <inheritdoc />
		public MainViewModel(ILogger logger)
			: base(logger)
		{
		}

		public INavigator Navigator
		{
			get => _navigator ??= new Navigator();
			set => _navigator = value;
		}
	}
}