using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using essentialMix.Collections;
using essentialMix.Comparers;
using essentialMix.Data.Helpers;
using essentialMix.Data.Patterns.Provider;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Data.SQLite;

public class DataProvider :
	DataProvider<DbType>,
	IDataProvider<DbType>
{
	private static readonly Regex __columnSize = new Regex(@"\((?<size>\d+)\)", RegexHelper.OPTIONS_I);
	private static readonly Regex __cleanupValueTypeMess = new Regex(@"\(\d+\)|\d+$", RegexHelper.OPTIONS_I);

	/// <inheritdoc />
	public DataProvider()
	{
	}

	/// <inheritdoc />
	public DataProvider([NotNull] IDbConnection connection)
		: base(connection)
	{
	}

	public override DbProviderFactory Factory => SQLiteFactory.Instance;

	public override DbType DefaultDbType => DbType.Object;

	public virtual DataColumn ColumnFromSchemaRow([NotNull] DataRow row)
	{
		/*
		sample data on table using PRAGMA table_info(table_name);

		cid   name             type             notnull     dflt_value  pk
		----  ---------------  ---------------  ----------  ----------  ----------
		0     ip_from          int(10)          1                       1
		1     ip_to            int(10)          1                       2
		2     country_code     char(2)          1                       0
		3     country_name     varchar(64)      1                       0
		4     region_name      varchar(128)     0                       0
		5     city_name        varchar(128)     0                       0
		6     latitude         real             1                       0
		7     longitude        real             1                       0
		8     zip_code         varchar(30)      0                       0
		9     time_zone        varchar(8)       1                       0
		*/
		DataColumn newColumn;

		try
		{
			string type = Convert.ToString(row["type"]);
			newColumn = new DataColumn(Convert.ToString(row["name"]), MapType(type))
			{
				AllowDBNull = row["notnull"].To(0) == 0,
				DefaultValue = row["dflt_value"]
			};

			Match match = __columnSize.Match(type);
			if (match.Success) newColumn.MaxLength = match.Groups["size"].Value.To(0);
		}
		catch
		{
			newColumn = null;
		}

		return newColumn;
	}

	[NotNull]
	public virtual Type MapType(string value)
	{
		if (string.IsNullOrEmpty(value)) return typeof(object);
		value = value.Replace(string.Empty, __cleanupValueTypeMess);

		Type type = ((IDictionary<string, Type>)StringToTypeMapping).Match(value, StringFunctionalComparer.StartsWithOrdinalIgnoreCase).FirstOrDefault();
		return type ?? typeof(object);
	}

	protected override IReadOnlyDictionary<Type, DbType> TypeToTDbType => DbTypeHelper.TypeToTDbType;

	protected override IReadOnlyDictionary<DbType, Type> TdbTypeToType => DbTypeHelper.TdbTypeToType;

	protected override ISet<DbType> TextualTDbType => DbTypeHelper.TextualTDbType;

	protected IReadOnlyDictionary<string, Type> StringToTypeMapping = new Dictionary<string, Type>(StringFunctionalComparer.StartsWithOrdinalIgnoreCase)
	{
		{"BOOLEAN", typeof(bool)},
		{"INTEGER", typeof(int)},
		{"TINYINTEGER", typeof(int)},
		{"SMALLINTEGER", typeof(int)},
		{"MEDIUMINTEGER", typeof(int)},
		{"BIGINTEGER", typeof(int)},
		{"CHAR(1)", typeof(char)},
		{"CHARACTER(1)", typeof(char)},
		{"NCHAR(1)", typeof(char)},
		{"NCHARACTER(1)", typeof(char)},
		{"VARCHAR(1)", typeof(char)},
		{"VARCHARACTER(1)", typeof(char)},
		{"NVARCHAR(1)", typeof(char)},
		{"NVARCHARACTER(1)", typeof(char)},
		{"CHARACTER", typeof(string)},
		{"NCHARACTER", typeof(string)},
		{"VARCHARACTER", typeof(string)},
		{"NVARCHARACTER", typeof(string)},
		{"CLOB", typeof(string)},
		{"TEXT", typeof(string)},
		{"TINYTEXT", typeof(string)},
		{"MEDIUMTEXT", typeof(string)},
		{"LONGTEXT", typeof(string)},
		{"FLOAT", typeof(float)},
		{"REAL", typeof(double)},
		{"DOUBLE", typeof(double)},
		{"NUMERIC", typeof(decimal)},
		{"DECIMAL", typeof(decimal)},
		{"DATE", typeof(DateTime)},
		{"TIMESTAMP", typeof(DateTime)},
		{"BLOB", typeof(object)}
	}.AsReadOnly();
}