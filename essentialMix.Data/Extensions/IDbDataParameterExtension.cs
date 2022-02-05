using System;
using System.Data;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IDbDataParameterExtension
{
	[NotNull]
	public static IDbDataParameter Precision([NotNull] this IDbDataParameter thisValue, byte value)
	{
		thisValue.Precision = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Scale([NotNull] this IDbDataParameter thisValue, byte value)
	{
		thisValue.Scale = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Size([NotNull] this IDbDataParameter thisValue, int value)
	{
		thisValue.Size = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Type([NotNull] this IDbDataParameter thisValue, DbType value)
	{
		thisValue.DbType = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Direction([NotNull] this IDbDataParameter thisValue, ParameterDirection value)
	{
		thisValue.Direction = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Name([NotNull] this IDbDataParameter thisValue, string value, char defaultPrefix = ':')
	{
		value = value.ToNullIfEmpty()?.Prefix(defaultPrefix);
		if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(thisValue));
		thisValue.ParameterName = value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter SourceColumn([NotNull] this IDbDataParameter thisValue, string value)
	{
		thisValue.SourceColumn = value.ToNullIfEmpty();
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter Value([NotNull] this IDbDataParameter thisValue, object value)
	{
		thisValue.Value = value ?? DBNull.Value;
		return thisValue;
	}

	[NotNull]
	public static IDbDataParameter SourceVersion([NotNull] this IDbDataParameter thisValue, DataRowVersion value)
	{
		thisValue.SourceVersion = value;
		return thisValue;
	}
}