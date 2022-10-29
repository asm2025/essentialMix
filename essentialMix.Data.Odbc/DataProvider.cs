using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Numerics;
using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Data.Odbc;

public abstract class DataProvider
	: DataProvider<OdbcType>,
		IDataProvider<OdbcType>
{
	/// <inheritdoc />
	protected DataProvider()
	{
	}

	/// <inheritdoc />
	protected DataProvider([NotNull] IDbConnection connection)
		: base(connection)
	{
	}

	public override DbProviderFactory Factory => OdbcFactory.Instance;

	public override OdbcType DefaultDbType => OdbcType.VarBinary;

	protected override IReadOnlyDictionary<Type, OdbcType> TypeToTDbType { get; } =
		new Dictionary<Type, OdbcType>
		{
			{typeof(bool), OdbcType.Bit},
			{typeof(sbyte), OdbcType.TinyInt},
			{typeof(byte), OdbcType.TinyInt},
			{typeof(short), OdbcType.SmallInt},
			{typeof(ushort), OdbcType.SmallInt},
			{typeof(int), OdbcType.Int},
			{typeof(uint), OdbcType.Int},
			{typeof(long), OdbcType.BigInt},
			{typeof(ulong), OdbcType.BigInt},
			{typeof(BigInteger), OdbcType.BigInt},
			{typeof(float), OdbcType.Real},
			{typeof(double), OdbcType.Double},
			{typeof(decimal), OdbcType.Decimal},
			{typeof(string), OdbcType.NVarChar},
			{typeof(char[]), OdbcType.VarChar},
			{typeof(char), OdbcType.NChar},
			{typeof(DateTime), OdbcType.DateTime},
			{typeof(DateTimeOffset), OdbcType.DateTime},
			{typeof(TimeSpan), OdbcType.Time},
			{typeof(byte[]), OdbcType.Binary},
			{typeof(Guid), OdbcType.UniqueIdentifier},
			{typeof(object), OdbcType.VarBinary}
		}.AsReadOnly();

	protected override IReadOnlyDictionary<OdbcType, Type> TdbTypeToType { get; } =
		new Dictionary<OdbcType, Type>
		{
			{OdbcType.Bit, typeof(bool)},
			{OdbcType.TinyInt, typeof(byte)},
			{OdbcType.SmallInt, typeof(short)},
			{OdbcType.Int, typeof(int)},
			{OdbcType.BigInt, typeof(long)},
			{OdbcType.Real, typeof(float)},
			{OdbcType.Double, typeof(double)},
			{OdbcType.Decimal, typeof(decimal)},
			{OdbcType.Char, typeof(char)},
			{OdbcType.NChar, typeof(char)},
			{OdbcType.Text, typeof(string)},
			{OdbcType.VarChar, typeof(string)},
			{OdbcType.NText, typeof(string)},
			{OdbcType.NVarChar, typeof(string)},
			{OdbcType.UniqueIdentifier, typeof(Guid)},
			{OdbcType.DateTime, typeof(DateTime)},
			{OdbcType.SmallDateTime, typeof(DateTime)},
			{OdbcType.Date, typeof(DateTime)},
			{OdbcType.Time, typeof(DateTime)},
			{OdbcType.Binary, typeof(byte[])},
			{OdbcType.VarBinary, typeof(byte[])},
			{OdbcType.Image, typeof(byte[])},
			{OdbcType.Timestamp, typeof(byte[])}
		}.AsReadOnly();

	protected override ISet<OdbcType> TextualTDbType { get; } = new HashSet<OdbcType>
	{
		OdbcType.Char,
		OdbcType.NChar,
		OdbcType.Text,
		OdbcType.VarChar,
		OdbcType.NText,
		OdbcType.NVarChar,
		OdbcType.UniqueIdentifier,
		OdbcType.DateTime,
		OdbcType.SmallDateTime,
		OdbcType.Date,
		OdbcType.Time
	};
}