using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Numerics;
using essentialMix.Collections;
using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;

namespace essentialMix.Data.MySql
{
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
			new ReadOnlyDictionary<Type, MySqlDbType>(new Dictionary<Type, MySqlDbType>
			{
				new KeyValuePair<Type, MySqlDbType>(typeof(bool), MySqlDbType.Bit),
				new KeyValuePair<Type, MySqlDbType>(typeof(sbyte), MySqlDbType.Byte),
				new KeyValuePair<Type, MySqlDbType>(typeof(byte), MySqlDbType.UByte),
				new KeyValuePair<Type, MySqlDbType>(typeof(short), MySqlDbType.Int16),
				new KeyValuePair<Type, MySqlDbType>(typeof(ushort), MySqlDbType.UInt16),
				new KeyValuePair<Type, MySqlDbType>(typeof(int), MySqlDbType.Int32),
				new KeyValuePair<Type, MySqlDbType>(typeof(uint), MySqlDbType.Int32),
				new KeyValuePair<Type, MySqlDbType>(typeof(long), MySqlDbType.Int64),
				new KeyValuePair<Type, MySqlDbType>(typeof(ulong), MySqlDbType.UInt64),
				new KeyValuePair<Type, MySqlDbType>(typeof(BigInteger), MySqlDbType.Int64),
				new KeyValuePair<Type, MySqlDbType>(typeof(float), MySqlDbType.Float),
				new KeyValuePair<Type, MySqlDbType>(typeof(double), MySqlDbType.Double),
				new KeyValuePair<Type, MySqlDbType>(typeof(decimal), MySqlDbType.Decimal),
				new KeyValuePair<Type, MySqlDbType>(typeof(string), MySqlDbType.VarChar),
				new KeyValuePair<Type, MySqlDbType>(typeof(char[]), MySqlDbType.VarChar),
				new KeyValuePair<Type, MySqlDbType>(typeof(char), MySqlDbType.VarChar),
				new KeyValuePair<Type, MySqlDbType>(typeof(DateTime), MySqlDbType.DateTime),
				new KeyValuePair<Type, MySqlDbType>(typeof(DateTimeOffset), MySqlDbType.DateTime),
				new KeyValuePair<Type, MySqlDbType>(typeof(TimeSpan), MySqlDbType.Time),
				new KeyValuePair<Type, MySqlDbType>(typeof(byte[]), MySqlDbType.Binary),
				new KeyValuePair<Type, MySqlDbType>(typeof(Image), MySqlDbType.LongBlob),
				new KeyValuePair<Type, MySqlDbType>(typeof(Guid), MySqlDbType.Guid),
				new KeyValuePair<Type, MySqlDbType>(typeof(object), MySqlDbType.Blob)
			});

		protected override IReadOnlyDictionary<MySqlDbType, Type> TdbTypeToType { get; } =
			new ReadOnlyDictionary<MySqlDbType, Type>(new Dictionary<MySqlDbType, Type>
			{
				/*
				Some of these types are obsolete in the documentation!
	
				MySqlDbType.String
				MySqlDbType.Newdate
	
				For more information, see 
				https://dev.mysql.com/doc/connector-net/en/connector-net-ref-mysqlclient-mysqlcommandmembers.html#connector-net-ref-mysqlclient-mysqldbtype
				*/
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Bit, typeof(bool)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Byte, typeof(sbyte)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.UByte, typeof(byte)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Int16, typeof(short)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.UInt16, typeof(ushort)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Int24, typeof(int)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.UInt24, typeof(uint)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Int32, typeof(int)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.UInt32, typeof(uint)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Int64, typeof(long)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.UInt64, typeof(ulong)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Float, typeof(float)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Double, typeof(double)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Decimal, typeof(decimal)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.NewDecimal, typeof(decimal)), // obsolete in docs
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.VarChar, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.VarString, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.TinyText, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.MediumText, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.LongText, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Text, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.String, typeof(string)), // obsolete in docs
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Set, typeof(string)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.JSON, typeof(string)), // WTF!! not in docs!!!
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Enum, typeof(string)), // !!
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Guid, typeof(Guid)), // Not in the fucking docs!!!
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.DateTime, typeof(DateTime)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Date, typeof(DateTime)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Time, typeof(DateTime)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Timestamp, typeof(DateTime)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Newdate, typeof(DateTime)), // obsolete in docs
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Year, typeof(short)),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.TinyBlob, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.MediumBlob, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.LongBlob, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Blob, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Binary, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.VarBinary, typeof(byte[])),
				new KeyValuePair<MySqlDbType, Type>(MySqlDbType.Geometry, typeof(byte[])) // WTF is this? it has no description in the documentation!
			});

		protected override IReadOnlySet<MySqlDbType> TextualTDbType { get; } = new ReadOnlySet<MySqlDbType>(new HashSet<MySqlDbType>
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
		});
	}
}