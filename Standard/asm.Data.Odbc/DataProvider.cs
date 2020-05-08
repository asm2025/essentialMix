using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Drawing;
using System.Numerics;
using asm.Collections;
using asm.Data.Patterns.Provider;
using JetBrains.Annotations;

namespace asm.Data.Odbc
{
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
			new ReadOnlyDictionary<Type, OdbcType>(new Dictionary<Type, OdbcType>
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
				{typeof(Image), OdbcType.Image},
				{typeof(Guid), OdbcType.UniqueIdentifier},
				{typeof(object), OdbcType.VarBinary}
			});

		protected override IReadOnlyDictionary<OdbcType, Type> TdbTypeToType { get; } =
			new ReadOnlyDictionary<OdbcType, Type>(new Dictionary<OdbcType, Type>
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
			});

		protected override IReadOnlySet<OdbcType> TextualTDbType { get; } = new ReadOnlySet<OdbcType>(new HashSet<OdbcType>
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
		});
	}
}