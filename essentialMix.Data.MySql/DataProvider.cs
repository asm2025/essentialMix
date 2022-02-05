using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Numerics;
using essentialMix.Collections;
using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;

namespace essentialMix.Data.MySql;

public class DataProvider
	: DataProvider<MySqlDbType>, 
		IDataProvider<MySqlDbType>
{
	/// <inheritdoc />
	public DataProvider() 
	{
	}

	/// <inheritdoc />
	public DataProvider([NotNull] IDbConnection connection)
		: base(connection)
	{
	}

	public override DbProviderFactory Factory => MySqlClientFactory.Instance;

	public override MySqlDbType DefaultDbType => MySqlDbType.Blob;

	protected override IReadOnlyDictionary<Type, MySqlDbType> TypeToTDbType { get; } =
		new Dictionary<Type, MySqlDbType>
		{
			{typeof(bool), MySqlDbType.Bit},
			{typeof(sbyte), MySqlDbType.Byte},
			{typeof(byte), MySqlDbType.UByte},
			{typeof(short), MySqlDbType.Int16},
			{typeof(ushort), MySqlDbType.UInt16},
			{typeof(int), MySqlDbType.Int32},
			{typeof(uint), MySqlDbType.Int32},
			{typeof(long), MySqlDbType.Int64},
			{typeof(ulong), MySqlDbType.UInt64},
			{typeof(BigInteger), MySqlDbType.Int64},
			{typeof(float), MySqlDbType.Float},
			{typeof(double), MySqlDbType.Double},
			{typeof(decimal), MySqlDbType.Decimal},
			{typeof(string), MySqlDbType.VarChar},
			{typeof(char[]), MySqlDbType.VarChar},
			{typeof(char), MySqlDbType.VarChar},
			{typeof(DateTime), MySqlDbType.DateTime},
			{typeof(DateTimeOffset), MySqlDbType.DateTime},
			{typeof(TimeSpan), MySqlDbType.Time},
			{typeof(byte[]), MySqlDbType.Binary},
			{typeof(Image), MySqlDbType.LongBlob},
			{typeof(Guid), MySqlDbType.Guid},
			{typeof(object), MySqlDbType.Blob}
		}.AsReadOnly();

	protected override IReadOnlyDictionary<MySqlDbType, Type> TdbTypeToType { get; } =
		new Dictionary<MySqlDbType, Type>
		{
			/*
			Some of these types are obsolete in the documentation!

			MySqlDbType.String
			MySqlDbType.Newdate

			For more information, see 
			https://dev.mysql.com/doc/connector-net/en/connector-net-ref-mysqlclient-mysqlcommandmembers.html#connector-net-ref-mysqlclient-mysqldbtype
			*/
			{MySqlDbType.Bit, typeof(bool)},
			{MySqlDbType.Byte, typeof(sbyte)},
			{MySqlDbType.UByte, typeof(byte)},
			{MySqlDbType.Int16, typeof(short)},
			{MySqlDbType.UInt16, typeof(ushort)},
			{MySqlDbType.Int24, typeof(int)},
			{MySqlDbType.UInt24, typeof(uint)},
			{MySqlDbType.Int32, typeof(int)},
			{MySqlDbType.UInt32, typeof(uint)},
			{MySqlDbType.Int64, typeof(long)},
			{MySqlDbType.UInt64, typeof(ulong)},
			{MySqlDbType.Float, typeof(float)},
			{MySqlDbType.Double, typeof(double)},
			{MySqlDbType.Decimal, typeof(decimal)},
			{MySqlDbType.NewDecimal, typeof(decimal)}, // obsolete in docs
			{MySqlDbType.VarChar, typeof(string)},
			{MySqlDbType.VarString, typeof(string)},
			{MySqlDbType.TinyText, typeof(string)},
			{MySqlDbType.MediumText, typeof(string)},
			{MySqlDbType.LongText, typeof(string)},
			{MySqlDbType.Text, typeof(string)},
			{MySqlDbType.String, typeof(string)}, // obsolete in docs
			{MySqlDbType.Set, typeof(string)},
			{MySqlDbType.JSON, typeof(string)}, // WTF!! not in docs!!!
			{MySqlDbType.Enum, typeof(string)}, // !!
			{MySqlDbType.Guid, typeof(Guid)}, // Not in the fucking docs!!!
			{MySqlDbType.DateTime, typeof(DateTime)},
			{MySqlDbType.Date, typeof(DateTime)},
			{MySqlDbType.Time, typeof(DateTime)},
			{MySqlDbType.Timestamp, typeof(DateTime)},
			{MySqlDbType.Newdate, typeof(DateTime)}, // obsolete in docs
			{MySqlDbType.Year, typeof(short)},
			{MySqlDbType.TinyBlob, typeof(byte[])},
			{MySqlDbType.MediumBlob, typeof(byte[])},
			{MySqlDbType.LongBlob, typeof(byte[])},
			{MySqlDbType.Blob, typeof(byte[])},
			{MySqlDbType.Binary, typeof(byte[])},
			{MySqlDbType.VarBinary, typeof(byte[])},
			{MySqlDbType.Geometry, typeof(byte[])} // WTF is this? it has no description in the documentation!
		}.AsReadOnly();

	protected override IReadOnlySet<MySqlDbType> TextualTDbType { get; } = new HashSet<MySqlDbType>
	{
		MySqlDbType.VarChar,
		MySqlDbType.VarString,
		MySqlDbType.TinyText,
		MySqlDbType.MediumText,
		MySqlDbType.LongText,
		MySqlDbType.Text,
		MySqlDbType.String, // obsolete in docs
		MySqlDbType.Set,
		MySqlDbType.JSON, // WTF!! not in docs!!!
		MySqlDbType.Enum, // !!
		MySqlDbType.Guid, // Not in the fucking docs!!!
		MySqlDbType.DateTime,
		MySqlDbType.Date,
		MySqlDbType.Time,
		MySqlDbType.Timestamp,
		MySqlDbType.Newdate // obsolete in docs
	}.AsReadOnly();
}