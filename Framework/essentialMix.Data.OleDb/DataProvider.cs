using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Numerics;
using essentialMix.Collections;
using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Data.OleDb;

public abstract class DataProvider
	: DataProvider<OleDbType>,
		IDataProvider<OleDbType>
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

	public override DbProviderFactory Factory => OleDbFactory.Instance;

	public override OleDbType DefaultDbType => OleDbType.Empty;

	public override string FormatValue<T>(T value, OleDbType dbType)
	{
		if (value.IsNull()) return "NULL";

		switch (dbType)
		{
			case OleDbType.DBTimeStamp:
				return string.Concat("'", $"{value:yyyymmddhhmmss}", "'");
			case OleDbType.DBDate:
				return string.Concat("'", $"{value:yyyymmdd}", "'");
			case OleDbType.DBTime:
				return string.Concat("'", $"{value:hhmmss}", "'");
			case OleDbType.Date:
				return Convert.ToDateTime(value).OAValue().ToString(CultureInfo.InvariantCulture);
			case OleDbType.Filetime:
				return Convert.ToDateTime(value).OAValue().ToLong().ToString(CultureInfo.InvariantCulture);
			default:
				return base.FormatValue(value, dbType);
		}
	}

	protected override IReadOnlyDictionary<Type, OleDbType> TypeToTDbType { get; } =
		new Dictionary<Type, OleDbType>
		{
			{typeof(bool), OleDbType.Boolean},
			{typeof(sbyte), OleDbType.TinyInt},
			{typeof(byte), OleDbType.UnsignedTinyInt},
			{typeof(short), OleDbType.SmallInt},
			{typeof(ushort), OleDbType.UnsignedSmallInt},
			{typeof(int), OleDbType.Integer},
			{typeof(uint), OleDbType.UnsignedInt},
			{typeof(long), OleDbType.BigInt},
			{typeof(ulong), OleDbType.UnsignedBigInt},
			{typeof(BigInteger), OleDbType.BigInt},
			{typeof(float), OleDbType.Single},
			{typeof(double), OleDbType.Double},
			{typeof(decimal), OleDbType.Decimal},
			{typeof(string), OleDbType.VarWChar},
			{typeof(char[]), OleDbType.VarChar},
			{typeof(char), OleDbType.Char},
			{typeof(DateTime), OleDbType.DBTimeStamp},
			{typeof(DateTimeOffset), OleDbType.Date},
			{typeof(TimeSpan), OleDbType.DBTime},
			{typeof(Guid), OleDbType.Guid},
			{typeof(byte[]), OleDbType.Binary},
			{typeof(Exception), OleDbType.Error},
			{typeof(object), OleDbType.Variant}
		}.AsReadOnly();

	protected override IReadOnlyDictionary<OleDbType, Type> TdbTypeToType { get; } =
		new Dictionary<OleDbType, Type>
		{
			new KeyValuePair<OleDbType, Type>(OleDbType.Boolean, typeof(bool)),
			new KeyValuePair<OleDbType, Type>(OleDbType.TinyInt, typeof(sbyte)),
			new KeyValuePair<OleDbType, Type>(OleDbType.UnsignedTinyInt, typeof(byte)),
			new KeyValuePair<OleDbType, Type>(OleDbType.SmallInt, typeof(short)),
			new KeyValuePair<OleDbType, Type>(OleDbType.UnsignedSmallInt, typeof(ushort)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Integer, typeof(int)),
			new KeyValuePair<OleDbType, Type>(OleDbType.UnsignedInt, typeof(uint)),
			new KeyValuePair<OleDbType, Type>(OleDbType.BigInt, typeof(long)),
			new KeyValuePair<OleDbType, Type>(OleDbType.UnsignedBigInt, typeof(ulong)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Single, typeof(float)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Double, typeof(double)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Decimal, typeof(decimal)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Currency, typeof(decimal)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Numeric, typeof(decimal)),
			new KeyValuePair<OleDbType, Type>(OleDbType.VarNumeric, typeof(decimal)),
			new KeyValuePair<OleDbType, Type>(OleDbType.BSTR, typeof(string)),
			new KeyValuePair<OleDbType, Type>(OleDbType.VarChar, typeof(string)),
			new KeyValuePair<OleDbType, Type>(OleDbType.VarWChar, typeof(string)),
			new KeyValuePair<OleDbType, Type>(OleDbType.LongVarChar, typeof(string)),
			new KeyValuePair<OleDbType, Type>(OleDbType.LongVarWChar, typeof(string)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Char, typeof(char)),
			new KeyValuePair<OleDbType, Type>(OleDbType.WChar, typeof(char)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Guid, typeof(Guid)),

			new KeyValuePair<OleDbType, Type>(OleDbType.DBTimeStamp, typeof(DateTime)),
			new KeyValuePair<OleDbType, Type>(OleDbType.DBDate, typeof(DateTime)),
			new KeyValuePair<OleDbType, Type>(OleDbType.DBTime, typeof(TimeSpan)),

			new KeyValuePair<OleDbType, Type>(OleDbType.Date, typeof(DateTime)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Filetime, typeof(DateTime)),

			new KeyValuePair<OleDbType, Type>(OleDbType.Binary, typeof(byte[])),
			new KeyValuePair<OleDbType, Type>(OleDbType.VarBinary, typeof(byte[])),
			new KeyValuePair<OleDbType, Type>(OleDbType.LongVarBinary, typeof(byte[])),
			new KeyValuePair<OleDbType, Type>(OleDbType.Error, typeof(Exception)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Variant, typeof(object)),
			new KeyValuePair<OleDbType, Type>(OleDbType.PropVariant, typeof(object)),
			new KeyValuePair<OleDbType, Type>(OleDbType.IUnknown, typeof(object)),
			new KeyValuePair<OleDbType, Type>(OleDbType.IDispatch, typeof(object)),
			new KeyValuePair<OleDbType, Type>(OleDbType.Empty, null)
		}.AsReadOnly();

	protected override IReadOnlySet<OleDbType> TextualTDbType { get; } = new HashSet<OleDbType>
	{
		OleDbType.BSTR,
		OleDbType.VarChar,
		OleDbType.VarWChar,
		OleDbType.LongVarChar,
		OleDbType.LongVarWChar,
		OleDbType.Char,
		OleDbType.WChar,
		OleDbType.Guid,
		OleDbType.DBTimeStamp,
		OleDbType.DBDate,
		OleDbType.DBTime
	}.AsReadOnly();
}