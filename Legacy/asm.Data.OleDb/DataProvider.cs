using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Numerics;
using asm.Collections;
using asm.Data.Patterns.Provider;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.OleDb
{
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
			new ReadOnlyDictionary<Type, OleDbType>(new Dictionary<Type, OleDbType>
			{
				new KeyValuePair<Type, OleDbType>(typeof(bool), OleDbType.Boolean),
				new KeyValuePair<Type, OleDbType>(typeof(sbyte), OleDbType.TinyInt),
				new KeyValuePair<Type, OleDbType>(typeof(byte), OleDbType.UnsignedTinyInt),
				new KeyValuePair<Type, OleDbType>(typeof(short), OleDbType.SmallInt),
				new KeyValuePair<Type, OleDbType>(typeof(ushort), OleDbType.UnsignedSmallInt),
				new KeyValuePair<Type, OleDbType>(typeof(int), OleDbType.Integer),
				new KeyValuePair<Type, OleDbType>(typeof(uint), OleDbType.UnsignedInt),
				new KeyValuePair<Type, OleDbType>(typeof(long), OleDbType.BigInt),
				new KeyValuePair<Type, OleDbType>(typeof(ulong), OleDbType.UnsignedBigInt),
				new KeyValuePair<Type, OleDbType>(typeof(BigInteger), OleDbType.BigInt),
				new KeyValuePair<Type, OleDbType>(typeof(float), OleDbType.Single),
				new KeyValuePair<Type, OleDbType>(typeof(double), OleDbType.Double),
				new KeyValuePair<Type, OleDbType>(typeof(decimal), OleDbType.Decimal),
				new KeyValuePair<Type, OleDbType>(typeof(string), OleDbType.VarWChar),
				new KeyValuePair<Type, OleDbType>(typeof(char[]), OleDbType.VarChar),
				new KeyValuePair<Type, OleDbType>(typeof(char), OleDbType.Char),
				new KeyValuePair<Type, OleDbType>(typeof(DateTime), OleDbType.DBTimeStamp),
				new KeyValuePair<Type, OleDbType>(typeof(DateTimeOffset), OleDbType.Date),
				new KeyValuePair<Type, OleDbType>(typeof(TimeSpan), OleDbType.DBTime),
				new KeyValuePair<Type, OleDbType>(typeof(Guid), OleDbType.Guid),
				new KeyValuePair<Type, OleDbType>(typeof(byte[]), OleDbType.Binary),
				new KeyValuePair<Type, OleDbType>(typeof(Exception), OleDbType.Error),
				new KeyValuePair<Type, OleDbType>(typeof(object), OleDbType.Variant)
			});

		protected override IReadOnlyDictionary<OleDbType, Type> TdbTypeToType { get; } =
			new ReadOnlyDictionary<OleDbType, Type>(new Dictionary<OleDbType, Type>
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
			});

		protected override IReadOnlySet<OleDbType> TextualTDbType { get; } = new ReadOnlySet<OleDbType>(new HashSet<OleDbType>
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
		});
	}
}