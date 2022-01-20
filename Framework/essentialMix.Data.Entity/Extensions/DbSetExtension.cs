using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DbSetExtension
{
	private static readonly ConcurrentDictionary<Type, FieldInfo> __fieldCache = new ConcurrentDictionary<Type, FieldInfo>();
	private static readonly ConcurrentDictionary<Type, PropertyInfo> __propertiesCache = new ConcurrentDictionary<Type, PropertyInfo>();

	public static DbContext GetContext<T>([NotNull] this DbSet<T> thisValue)
		where T : class
	{
		Type type = thisValue.GetType();
		FieldInfo internalSetField = __fieldCache.GetOrAdd(type, t => t.GetField("_internalSet", BindingFlags.NonPublic | BindingFlags.Instance));
		if (internalSetField == null) return null;

		object internalSet = internalSetField.GetValue(thisValue);
		Type contextType = internalSet?.GetType().BaseType;
		if (contextType == null) return null;

		FieldInfo internalContextField = __fieldCache.GetOrAdd(contextType, t => t.GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance));
		if (internalContextField == null) return null;

		object internalContext = internalContextField.GetValue(thisValue);
		if (internalContext == null) return null;

		PropertyInfo ownerProperty = __propertiesCache.GetOrAdd(internalContext.GetType(), t => t.GetProperty("Owner", BindingFlags.Instance | BindingFlags.Public));
		if (ownerProperty == null) return null;
		return (DbContext)ownerProperty.GetValue(internalContext, null);
	}

	public static string GetTableName<T>([NotNull] this DbSet<T> thisValue)
		where T : class
	{
		DbContext context = GetContext(thisValue);
		return context?.GetTableName(thisValue.GetType());
	}
}