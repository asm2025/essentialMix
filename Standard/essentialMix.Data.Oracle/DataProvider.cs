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
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace essentialMix.Data.Oracle;

public class DataProvider
	: DataProvider<OracleDbType>,
		IDataProvider<OracleDbType>
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

	public override DbProviderFactory Factory => OracleClientFactory.Instance;

	public override OracleDbType DefaultDbType => OracleDbType.Blob;

	protected override IReadOnlyDictionary<Type, OracleDbType> TypeToTDbType { get; } =
		new Dictionary<Type, OracleDbType>
		{
			{typeof(bool), OracleDbType.Char},
			{typeof(sbyte), OracleDbType.Byte},
			{typeof(byte), OracleDbType.Byte},
			{typeof(short), OracleDbType.Int16},
			{typeof(ushort), OracleDbType.Int16},
			{typeof(int), OracleDbType.Int32},
			{typeof(uint), OracleDbType.Int32},
			{typeof(long), OracleDbType.Int64},
			{typeof(ulong), OracleDbType.Int64},
			{typeof(BigInteger), OracleDbType.LongRaw},
			{typeof(float), OracleDbType.Single},
			{typeof(double), OracleDbType.Double},
			{typeof(decimal), OracleDbType.Decimal},
			{typeof(OracleDecimal), OracleDbType.Decimal},
			{typeof(string), OracleDbType.NVarchar2},
			{typeof(char[]), OracleDbType.Varchar2},
			{typeof(OracleString), OracleDbType.Varchar2},
			{typeof(char), OracleDbType.Char},
			{typeof(Guid), OracleDbType.Varchar2},
			{typeof(OracleXmlType), OracleDbType.XmlType},
			{typeof(OracleClob), OracleDbType.NClob},
			{typeof(OracleDate), OracleDbType.Date},
			{typeof(DateTime), OracleDbType.TimeStamp},
			{typeof(OracleTimeStamp), OracleDbType.TimeStamp},
			{typeof(OracleTimeStampLTZ), OracleDbType.TimeStampLTZ},
			{typeof(OracleTimeStampTZ), OracleDbType.TimeStampTZ},
			{typeof(DateTimeOffset), OracleDbType.Date},
			{typeof(TimeSpan), OracleDbType.IntervalDS},
			{typeof(OracleIntervalDS), OracleDbType.IntervalDS},
			{typeof(OracleIntervalYM), OracleDbType.IntervalYM},
			{typeof(OracleRefCursor), OracleDbType.RefCursor},
			{typeof(byte[]), OracleDbType.Blob},
			{typeof(Image), OracleDbType.BFile},
			{typeof(OracleBFile), OracleDbType.BFile},
			{typeof(OracleBinary), OracleDbType.Raw},
			{typeof(object), OracleDbType.Blob},
			{typeof(OracleBlob), OracleDbType.Blob}
		}.AsReadOnly();

	protected override IReadOnlyDictionary<OracleDbType, Type> TdbTypeToType { get; } =
		new Dictionary<OracleDbType, Type>
		{
			{OracleDbType.Byte, typeof(byte)},
			{OracleDbType.Int16, typeof(short)},
			{OracleDbType.Int32, typeof(int)},
			{OracleDbType.Int64, typeof(long)},
			{OracleDbType.Long, typeof(long)},
			{OracleDbType.Single, typeof(float)},
			{OracleDbType.Double, typeof(double)},
			{OracleDbType.Decimal, typeof(decimal)},
			{OracleDbType.LongRaw, typeof(byte[])},
			{OracleDbType.BinaryFloat, typeof(byte[])},
			{OracleDbType.BinaryDouble, typeof(byte[])},
			{OracleDbType.Char, typeof(char)},
			{OracleDbType.NChar, typeof(char)},
			{OracleDbType.Varchar2, typeof(string)},
			{OracleDbType.NVarchar2, typeof(string)},
			{OracleDbType.XmlType, typeof(OracleXmlType)},
			{OracleDbType.Clob, typeof(OracleClob)},
			{OracleDbType.NClob, typeof(OracleClob)},
			{OracleDbType.Date, typeof(OracleDate)},
			{OracleDbType.IntervalDS, typeof(OracleIntervalDS)},
			{OracleDbType.TimeStamp, typeof(OracleTimeStamp)},
			{OracleDbType.TimeStampLTZ, typeof(OracleTimeStampLTZ)},
			{OracleDbType.TimeStampTZ, typeof(OracleTimeStampTZ)},
			{OracleDbType.IntervalYM, typeof(OracleIntervalYM)},
			{OracleDbType.RefCursor, typeof(OracleRefCursor)},
			{OracleDbType.Raw, typeof(byte[])},
			{OracleDbType.BFile, typeof(byte[])},
			{OracleDbType.Blob, typeof(byte[])}
		}.AsReadOnly();

	protected override IReadOnlySet<OracleDbType> TextualTDbType { get; } = new HashSet<OracleDbType>
	{
		OracleDbType.Char,
		OracleDbType.NChar,
		OracleDbType.Varchar2,
		OracleDbType.NVarchar2,
		OracleDbType.XmlType,
		OracleDbType.Clob,
		OracleDbType.NClob,
		OracleDbType.Date,
		OracleDbType.IntervalDS,
		OracleDbType.TimeStamp,
		OracleDbType.TimeStampLTZ,
		OracleDbType.TimeStampTZ
	}.AsReadOnly();
}