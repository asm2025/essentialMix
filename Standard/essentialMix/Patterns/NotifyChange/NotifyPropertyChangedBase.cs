using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace essentialMix.Patterns.NotifyChange
{
	public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		protected NotifyPropertyChangedBase() 
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs args)
		{
			PropertyChanged?.Invoke(this, args);
		}

		[NotifyPropertyChangedInvocator]
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged == null) return;
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
	}
}
