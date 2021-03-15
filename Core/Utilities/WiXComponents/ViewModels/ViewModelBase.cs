using System.Threading;
using essentialMix.Patterns.NotifyChange;
using Microsoft.Extensions.Logging;

namespace WiXComponents.ViewModels
{
	public abstract class ViewModelBase : NotifyPropertyChangedBase
	{
		private volatile int _isBusy;

		protected ViewModelBase(ILogger logger)
		{
			Logger = logger;
		}

		public bool IsBusy
		{
			get => _isBusy != 0;
			protected set
			{
				Interlocked.CompareExchange(ref _isBusy, value
															? 1
															: 0, _isBusy);
				OnPropertyChanged();
			}
		}

		public ILogger Logger { get; }
	}
}