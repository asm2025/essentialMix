using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Data.Helpers;

public static class DbTypeHelper
{
	public static IReadOnlyDictionary<Type, DbType> TypeToTDbType { get; } = new Dictionary<Type, DbType>
	{
		{typeof(bool), DbType.Boolean},
		{typeof(sbyte), DbType.SByte},
		{typeof(byte), DbType.Byte},
		{typeof(short), DbType.Int16},
		{typeof(ushort), DbType.UInt16},
		{typeof(int), DbType.Int32},
		{typeof(uint), DbType.UInt32},
		{typeof(long), DbType.Int64},
		{typeof(ulong), DbType.UInt64},
		{typeof(BigInteger), DbType.Int64},
		{typeof(float), DbType.Single},
		{typeof(double), DbType.Double},
		{typeof(decimal), DbType.Decimal},
		{typeof(string), DbType.String},
		{typeof(char[]), DbType.AnsiString},
		{typeof(char), DbType.StringFixedLength},
		{typeof(Guid), DbType.Guid},
		{typeof(DateTime), DbType.DateTime},
		{typeof(DateTimeOffset), DbType.DateTimeOffset},
		{typeof(byte[]), DbType.Binary}
	}.AsReadOnly();

	public static IReadOnlyDictionary<DbType, Type> TdbTypeToType { get; } = new Dictionary<DbType, Type>
	{
		{DbType.Boolean, typeof(bool)},
		{DbType.SByte, typeof(sbyte)},
		{DbType.Byte, typeof(byte)},
		{DbType.Int16, typeof(short)},
		{DbType.UInt16, typeof(ushort)},
		{DbType.Int32, typeof(int)},
		{DbType.UInt32, typeof(uint)},
		{DbType.Int64, typeof(long)},
		{DbType.UInt64, typeof(ulong)},
		{DbType.Single, typeof(float)},
		{DbType.Double, typeof(double)},
		{DbType.Decimal, typeof(decimal)},
		{DbType.Currency, typeof(decimal)},
		{DbType.VarNumeric, typeof(decimal)},
		{DbType.AnsiStringFixedLength, typeof(char[])},
		{DbType.StringFixedLength, typeof(char[])},
		{DbType.AnsiString, typeof(string)},
		{DbType.String, typeof(string)},
		{DbType.Xml, typeof(string)},
		{DbType.Guid, typeof(Guid)},
		{DbType.DateTime, typeof(DateTime)},
		{DbType.DateTime2, typeof(DateTime)},
		{DbType.Date, typeof(DateTime)},
		{DbType.Time, typeof(DateTime)},
		{DbType.DateTimeOffset, typeof(DateTimeOffset)},
		{DbType.Binary, typeof(byte[])},
		{DbType.Object, typeof(object)}
	}.AsReadOnly();

	public static ISet<DbType> TextualTDbType { get; } = new HashSet<DbType>
	{
		DbType.AnsiStringFixedLength,
		DbType.StringFixedLength,
		DbType.AnsiString,
		DbType.String,
		DbType.Xml,
		DbType.Guid,
		DbType.DateTime,
		DbType.DateTime2,
		DbType.Date,
		DbType.Time,
		DbType.DateTimeOffset
	};

	[NotNull]
	public static string FormatValue([NotNull] DataRow row, [NotNull] DataColumn column) { return FormatValue(row[column.ColumnName], MapType(column.DataType)); }

	[NotNull]
	public static string FormatValue<T>(T value, DbType dbType)
	{
		if (value.IsNull()) return "NULL";
		return IsTextual(dbType) ? string.Concat("'", value, "'") : Convert.ToString(value);
	}

	public static Type MapType(DbType value)
	{
		return TdbTypeToType.TryGetValue(value, out Type type)
					? type
					: null;
	}

	public static DbType MapType([NotNull] Type value)
	{
		return TypeToTDbType.TryGetValue(value, out DbType type)
					? type
					: DbType.Object;
	}

	public static bool IsTextual(DbType type) { return TextualTDbType.Contains(type); }
}