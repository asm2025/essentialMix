using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Numerics;
using asm.Collections;
using asm.Data.Patterns.Provider;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.MSSQL
{
	public class DataProvider 
		: DataProvider<SqlDbType>, 
		IDataProvider<SqlDbType>
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

		public override DbProviderFactory Factory => SqlClientFactory.Instance;

		public override SqlDbType DefaultDbType => SqlDbType.Variant;

		protected override IReadOnlyDictionary<Type, SqlDbType> TypeToTDbType { get; } =
			new ReadOnlyDictionary<Type, SqlDbType>(new Dictionary<Type, SqlDbType>
			{
				new KeyValuePair<Type, SqlDbType>(typeof(bool), SqlDbType.Bit),
				new KeyValuePair<Type, SqlDbType>(typeof(sbyte), SqlDbType.TinyInt),
				new KeyValuePair<Type, SqlDbType>(typeof(byte), SqlDbType.TinyInt),
				new KeyValuePair<Type, SqlDbType>(typeof(short), SqlDbType.SmallInt),
				new KeyValuePair<Type, SqlDbType>(typeof(ushort), SqlDbType.SmallInt),
				new KeyValuePair<Type, SqlDbType>(typeof(int), SqlDbType.Int),
				new KeyValuePair<Type, SqlDbType>(typeof(uint), SqlDbType.Int),
				new KeyValuePair<Type, SqlDbType>(typeof(long), SqlDbType.BigInt),
				new KeyValuePair<Type, SqlDbType>(typeof(ulong), SqlDbType.BigInt),
				new KeyValuePair<Type, SqlDbType>(typeof(BigInteger), SqlDbType.BigInt),
				new KeyValuePair<Type, SqlDbType>(typeof(float), SqlDbType.Real),
				new KeyValuePair<Type, SqlDbType>(typeof(double), SqlDbType.Float),
				new KeyValuePair<Type, SqlDbType>(typeof(decimal), SqlDbType.Decimal),
				new KeyValuePair<Type, SqlDbType>(typeof(string), SqlDbType.NVarChar),
				new KeyValuePair<Type, SqlDbType>(typeof(char[]), SqlDbType.VarChar),
				new KeyValuePair<Type, SqlDbType>(typeof(char), SqlDbType.NChar),
				new KeyValuePair<Type, SqlDbType>(typeof(DateTime), SqlDbType.DateTime),
				new KeyValuePair<Type, SqlDbType>(typeof(DateTimeOffset), SqlDbType.DateTimeOffset),
				new KeyValuePair<Type, SqlDbType>(typeof(TimeSpan), SqlDbType.Time),
				new KeyValuePair<Type, SqlDbType>(typeof(byte[]), SqlDbType.Binary),
				new KeyValuePair<Type, SqlDbType>(typeof(Image), SqlDbType.Image),
				new KeyValuePair<Type, SqlDbType>(typeof(Guid), SqlDbType.UniqueIdentifier),
				new KeyValuePair<Type, SqlDbType>(typeof(object), SqlDbType.Variant)
			});

		protected override IReadOnlyDictionary<SqlDbType, Type> TdbTypeToType { get; } =
			new ReadOnlyDictionary<SqlDbType, Type>(new Dictionary<SqlDbType, Type>
			{
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Bit, typeof(bool)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.TinyInt, typeof(byte)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.SmallInt, typeof(short)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Int, typeof(int)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.BigInt, typeof(long)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Real, typeof(float)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Float, typeof(double)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Decimal, typeof(decimal)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.SmallMoney, typeof(decimal)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Money, typeof(decimal)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Char, typeof(char)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.NChar, typeof(char)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Text, typeof(string)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.VarChar, typeof(string)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.NText, typeof(string)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.NVarChar, typeof(string)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Xml, typeof(string)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.UniqueIdentifier, typeof(Guid)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.DateTime, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.DateTime2, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.SmallDateTime, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Date, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Time, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.DateTimeOffset, typeof(DateTime)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Binary, typeof(byte[])),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.VarBinary, typeof(byte[])),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Image, typeof(byte[])),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Timestamp, typeof(byte[])),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Variant, typeof(object)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Udt, typeof(object)),
				new KeyValuePair<SqlDbType, Type>(SqlDbType.Structured, typeof(object))
			});

		protected override IReadOnlySet<SqlDbType> TextualTDbType { get; } = new ReadOnlySet<SqlDbType>(new HashSet<SqlDbType>
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
		});
	}
}