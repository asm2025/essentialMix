using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace essentialMix.Logging.Helpers;

public static class LogFactoryHelper
{
	private static readonly Lazy<ILoggerFactory> __consoleFactory = new Lazy<ILoggerFactory>(() => LoggerFactory.Create(builder => builder.AddConsole()), LazyThreadSafetyMode.PublicationOnly);

	private static readonly Type __targetType = typeof(ILoggerFactory);
	private static readonly ConcurrentDictionary<Type, Func<ILoggerFactory>> __templates = new ConcurrentDictionary<Type, Func<ILoggerFactory>>();
	private static readonly ConcurrentDictionary<Type, ILoggerFactory> __instances = new ConcurrentDictionary<Type, ILoggerFactory>();

	[NotNull]
	public static ILoggerFactory Empty => NullLoggerFactory.Instance;

	[NotNull]
	public static ILoggerFactory ConsoleLoggerFactory => __consoleFactory.Value;

	public static void Register<T>([NotNull] Expression<Func<T>> template)
		where T : ILoggerFactory
	{
		Expression castExpr = Expression.Convert(template.Body, typeof(ILoggerFactory));
		Register(typeof(T), Expression.Lambda<Func<ILoggerFactory>>(castExpr));
	}

	public static void Register([NotNull] Type type, [NotNull] Expression<Func<ILoggerFactory>> template)
	{
		AssertType(type);
		__templates.TryAdd(type, template.Compile());
	}

	public static void Unregister<T>()
		where T : ILoggerFactory
	{
		Unregister(typeof(T));
	}

	public static void Unregister([NotNull] Type type)
	{
		__templates.TryRemove(type, out _);
		__instances.TryRemove(type, out _);
	}

	public static void ClearRegistration()
	{
		__templates.Clear();
	}

	[NotNull]
	public static T GetOrCreate<T>()
		where T : ILoggerFactory
	{
		return (T)GetOrCreate(typeof(T));
	}

	[NotNull]
	public static ILoggerFactory GetOrCreate(Type type = null)
	{
		return (type == null || type == Empty.GetType()
					? Empty
					: __instances.TryGetValue(type, out ILoggerFactory instance)
						? instance
						: !__templates.TryGetValue(type, out Func<ILoggerFactory> template)
							? null
							: __instances.GetOrAdd(type, _ => template())) ?? throw new InvalidOperationException("Type is not registered or created.");
	}

	public static void Clear()
	{
		__instances.Clear();
	}

	private static void AssertType(Type type)
	{
		if (__targetType.IsAssignableFrom(type)) return;
		throw new ArgumentException($"{type} does not implement {nameof(ILoggerFactory)} interface.", nameof(type));
	}
}