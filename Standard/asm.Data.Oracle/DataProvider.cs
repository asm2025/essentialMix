using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Numerics;
using asm.Collections;
using asm.Data.Patterns.Provider;
using asm.Extensions;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace asm.Data.Oracle
{
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
			new ReadOnlyDictionary<Type, OracleDbType>(new Dictionary<Type, OracleDbType>
			{
				new KeyValuePair<Type, OracleDbType>(typeof(bool), OracleDbType.Char),
				new KeyValuePair<Type, OracleDbType>(typeof(sbyte), OracleDbType.Byte),
				new KeyValuePair<Type, OracleDbType>(typeof(byte), OracleDbType.Byte),
				new KeyValuePair<Type, OracleDbType>(typeof(short), OracleDbType.Int16),
				new KeyValuePair<Type, OracleDbType>(typeof(ushort), OracleDbType.Int16),
				new KeyValuePair<Type, OracleDbType>(typeof(int), OracleDbType.Int32),
				new KeyValuePair<Type, OracleDbType>(typeof(uint), OracleDbType.Int32),
				new KeyValuePair<Type, OracleDbType>(typeof(long), OracleDbType.Int64),
				new KeyValuePair<Type, OracleDbType>(typeof(ulong), OracleDbType.Int64),
				new KeyValuePair<Type, OracleDbType>(typeof(BigInteger), OracleDbType.LongRaw),
				new KeyValuePair<Type, OracleDbType>(typeof(float), OracleDbType.Single),
				new KeyValuePair<Type, OracleDbType>(typeof(double), OracleDbType.Double),
				new KeyValuePair<Type, OracleDbType>(typeof(decimal), OracleDbType.Decimal),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleDecimal), OracleDbType.Decimal),
				new KeyValuePair<Type, OracleDbType>(typeof(string), OracleDbType.NVarchar2),
				new KeyValuePair<Type, OracleDbType>(typeof(char[]), OracleDbType.Varchar2),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleString), OracleDbType.Varchar2),
				new KeyValuePair<Type, OracleDbType>(typeof(char), OracleDbType.Char),
				new KeyValuePair<Type, OracleDbType>(typeof(Guid), OracleDbType.Varchar2),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleXmlType), OracleDbType.XmlType),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleClob), OracleDbType.NClob),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleDate), OracleDbType.Date),
				new KeyValuePair<Type, OracleDbType>(typeof(DateTime), OracleDbType.TimeStamp),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleTimeStamp), OracleDbType.TimeStamp),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleTimeStampLTZ), OracleDbType.TimeStampLTZ),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleTimeStampTZ), OracleDbType.TimeStampTZ),
				new KeyValuePair<Type, OracleDbType>(typeof(DateTimeOffset), OracleDbType.Date),
				new KeyValuePair<Type, OracleDbType>(typeof(TimeSpan), OracleDbType.IntervalDS),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleIntervalDS), OracleDbType.IntervalDS),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleIntervalYM), OracleDbType.IntervalYM),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleRefCursor), OracleDbType.RefCursor),
				new KeyValuePair<Type, OracleDbType>(typeof(byte[]), OracleDbType.Blob),
				new KeyValuePair<Type, OracleDbType>(typeof(Image), OracleDbType.BFile),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleBFile), OracleDbType.BFile),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleBinary), OracleDbType.Raw),
				new KeyValuePair<Type, OracleDbType>(typeof(object), OracleDbType.Blob),
				new KeyValuePair<Type, OracleDbType>(typeof(OracleBlob), OracleDbType.Blob)
			});

		protected override IReadOnlyDictionary<OracleDbType, Type> TdbTypeToType { get; } =
			new ReadOnlyDictionary<OracleDbType, Type>(new Dictionary<OracleDbType, Type>
			{
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Byte, typeof(byte)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Int16, typeof(short)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Int32, typeof(int)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Int64, typeof(long)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Long, typeof(long)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Single, typeof(float)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Double, typeof(double)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Decimal, typeof(decimal)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.LongRaw, typeof(byte[])),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.BinaryFloat, typeof(byte[])),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.BinaryDouble, typeof(byte[])),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Char, typeof(char)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.NChar, typeof(char)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Varchar2, typeof(string)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.NVarchar2, typeof(string)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.XmlType, typeof(OracleXmlType)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Clob, typeof(OracleClob)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.NClob, typeof(OracleClob)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Date, typeof(OracleDate)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.IntervalDS, typeof(OracleIntervalDS)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.TimeStamp, typeof(OracleTimeStamp)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.TimeStampLTZ, typeof(OracleTimeStampLTZ)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.TimeStampTZ, typeof(OracleTimeStampTZ)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.IntervalYM, typeof(OracleIntervalYM)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.RefCursor, typeof(OracleRefCursor)),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Raw, typeof(byte[])),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.BFile, typeof(byte[])),
				new KeyValuePair<OracleDbType, Type>(OracleDbType.Blob, typeof(byte[]))
			});

		protected override IReadOnlySet<OracleDbType> TextualTDbType { get; } = new ReadOnlySet<OracleDbType>(new HashSet<OracleDbType>
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
		});
	}
}