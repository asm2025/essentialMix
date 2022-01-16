using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Core.WPF.Commands;

public interface IRelayCommandBase
{
	bool IsAutomaticRequeryDisabled { get; set; }
	void RaiseCanExecuteChanged();
}

public static class IRelayCommandBaseExtension
{
	// inspired by https://stackoverflow.com/questions/1751966/commandmanager-invalidaterequerysuggested-isnt-fast-enough-what-can-i-do#1857619
	[NotNull]
	public static TCmd ListenOn<TCmd, TObj, TProperty>([NotNull] this TCmd thisValue, [NotNull] TObj obj,
		[NotNull] Expression<Func<TObj, TProperty>> propertyExpression, Dispatcher dispatcher = null)
		where TCmd : IRelayCommandBase
		where TObj : INotifyPropertyChanged
	{
		string propertyName = propertyExpression.GetProperty().Name;
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] string propertyName, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] string propertyName1, [NotNull] string propertyName2, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName1 && e.PropertyName != propertyName2) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] string propertyName1, [NotNull] string propertyName2, [NotNull] string propertyName3, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName1 && e.PropertyName != propertyName2 && e.PropertyName != propertyName3) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] string propertyName1, [NotNull] string propertyName2, [NotNull] string propertyName3, [NotNull] string propertyName4, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName1 && e.PropertyName != propertyName2 && e.PropertyName != propertyName3 && e.PropertyName != propertyName4) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] string propertyName1, [NotNull] string propertyName2, [NotNull] string propertyName3, [NotNull] string propertyName4, [NotNull] string propertyName5, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		obj.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName != propertyName1 && e.PropertyName != propertyName2 && e.PropertyName != propertyName3 && e.PropertyName != propertyName4 && e.PropertyName != propertyName5) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] params string[] propertyNames)
		where T : IRelayCommand
	{
		return ListenOn(thisValue, Dispatcher.CurrentDispatcher, obj, propertyNames);
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, Dispatcher dispatcher, [NotNull] INotifyPropertyChanged obj, [NotNull] params string[] propertyNames)
		where T : IRelayCommand
	{
		if (propertyNames.Length == 0) return thisValue;

		HashSet<string> set = new HashSet<string>(propertyNames.SkipNullOrEmpty());
		obj.PropertyChanged += (_, e) =>
		{
			if (set.Contains(e.PropertyName)) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] IReadOnlyCollection<string> propertyNames, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		HashSet<string> set = propertyNames as HashSet<string> ?? new HashSet<string>(propertyNames.SkipNullOrEmpty());
		obj.PropertyChanged += (_, e) =>
		{
			if (set.Contains(e.PropertyName)) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}

	[NotNull]
	public static T ListenOn<T>([NotNull] this T thisValue, [NotNull] INotifyPropertyChanged obj, [NotNull] ICollection<string> propertyNames, Dispatcher dispatcher = null)
		where T : IRelayCommand
	{
		HashSet<string> set = propertyNames as HashSet<string> ?? new HashSet<string>(propertyNames.SkipNullOrEmpty());
		obj.PropertyChanged += (_, e) =>
		{
			if (set.Contains(e.PropertyName)) return;
			dispatcher ??= Dispatcher.CurrentDispatcher;

			if (dispatcher != null) dispatcher.Run(thisValue.RaiseCanExecuteChanged);
			else thisValue.RaiseCanExecuteChanged();
		};
		return thisValue;
	}
}