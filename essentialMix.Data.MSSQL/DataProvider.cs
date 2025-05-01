using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Microsoft.Data.SqlClient;

namespace essentialMix.Data.MSSQL;

public class DataProvider
	: DataProvider<SqlDbType>,
		IDataProvider<SqlDbType>
{
	/// <inheritdoc />
	public DataProvider()
		: base(SqlClientFactory.Instance)
	{
	}

	/// <inheritdoc />
	public DataProvider([NotNull] IDbConnection connection)
		: base(SqlClientFactory.Instance, connection)
	{
	}

	public override SqlDbType DefaultDbType => SqlDbType.Variant;

	protected override IReadOnlyDictionary<Type, SqlDbType> TypeToTDbType { get; } =
		new Dictionary<Type, SqlDbType>
		{
			{typeof(bool), SqlDbType.Bit},
			{typeof(sbyte), SqlDbType.TinyInt},
			{typeof(byte), SqlDbType.TinyInt},
			{typeof(short), SqlDbType.SmallInt},
			{typeof(ushort), SqlDbType.SmallInt},
			{typeof(int), SqlDbType.Int},
			{typeof(uint), SqlDbType.Int},
			{typeof(long), SqlDbType.BigInt},
			{typeof(ulong), SqlDbType.BigInt},
			{typeof(BigInteger), SqlDbType.BigInt},
			{typeof(float), SqlDbType.Real},
			{typeof(double), SqlDbType.Float},
			{typeof(decimal), SqlDbType.Decimal},
			{typeof(string), SqlDbType.NVarChar},
			{typeof(char[]), SqlDbType.VarChar},
			{typeof(char), SqlDbType.NChar},
			{typeof(DateTime), SqlDbType.DateTime},
			{typeof(DateTimeOffset), SqlDbType.DateTimeOffset},
			{typeof(TimeSpan), SqlDbType.Time},
			{typeof(byte[]), SqlDbType.Binary},
			{typeof(Guid), SqlDbType.UniqueIdentifier},
			{typeof(object), SqlDbType.Variant}
		}.AsReadOnly();

	protected override IReadOnlyDictionary<SqlDbType, Type> TdbTypeToType { get; } =
		new Dictionary<SqlDbType, Type>
		{
			{SqlDbType.Bit, typeof(bool)},
			{SqlDbType.TinyInt, typeof(byte)},
			{SqlDbType.SmallInt, typeof(short)},
			{SqlDbType.Int, typeof(int)},
			{SqlDbType.BigInt, typeof(long)},
			{SqlDbType.Real, typeof(float)},
			{SqlDbType.Float, typeof(double)},
			{SqlDbType.Decimal, typeof(decimal)},
			{SqlDbType.SmallMoney, typeof(decimal)},
			{SqlDbType.Money, typeof(decimal)},
			{SqlDbType.Char, typeof(char)},
			{SqlDbType.NChar, typeof(char)},
			{SqlDbType.Text, typeof(string)},
			{SqlDbType.VarChar, typeof(string)},
			{SqlDbType.NText, typeof(string)},
			{SqlDbType.NVarChar, typeof(string)},
			{SqlDbType.Xml, typeof(string)},
			{SqlDbType.UniqueIdentifier, typeof(Guid)},
			{SqlDbType.DateTime, typeof(DateTime)},
			{SqlDbType.DateTime2, typeof(DateTime)},
			{SqlDbType.SmallDateTime, typeof(DateTime)},
			{SqlDbType.Date, typeof(DateTime)},
			{SqlDbType.Time, typeof(DateTime)},
			{SqlDbType.DateTimeOffset, typeof(DateTime)},
			{SqlDbType.Binary, typeof(byte[])},
			{SqlDbType.VarBinary, typeof(byte[])},
			{SqlDbType.Image, typeof(byte[])},
			{SqlDbType.Timestamp, typeof(byte[])},
			{SqlDbType.Variant, typeof(object)},
			{SqlDbType.Udt, typeof(object)},
			{SqlDbType.Structured, typeof(object)}
		}.AsReadOnly();

	protected override ISet<SqlDbType> TextualTDbType { get; } = new HashSet<SqlDbType>
	{
		SqlDbType.Char,
		SqlDbType.NChar,
		SqlDbType.Text,
		SqlDbType.VarChar,
		SqlDbType.NText,
		SqlDbType.NVarChar,
		SqlDbType.Xml,
		SqlDbType.UniqueIdentifier,
		SqlDbType.DateTime,
		SqlDbType.DateTime2,
		SqlDbType.SmallDateTime,
		SqlDbType.Date,
		SqlDbType.Time,
		SqlDbType.DateTimeOffset
	};
}