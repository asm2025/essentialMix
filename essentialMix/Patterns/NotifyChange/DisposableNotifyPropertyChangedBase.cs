using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.Patterns.NotifyChange;

public abstract class DisposableNotifyPropertyChangedBase : Disposable, INotifyPropertyChanged
{
	protected DisposableNotifyPropertyChangedBase()
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

	protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;
		field = newValue;
		OnPropertyChanged(propertyName);
		return true;
	}
}