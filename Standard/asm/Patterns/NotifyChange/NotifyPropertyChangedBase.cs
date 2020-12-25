using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace asm.Patterns.NotifyChange
{
	public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		protected NotifyPropertyChangedBase() 
		{
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs args) { PropertyChanged?.Invoke(this, args); }

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
