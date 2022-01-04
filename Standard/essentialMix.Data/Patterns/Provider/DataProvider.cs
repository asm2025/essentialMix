using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using essentialMix.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;
using essentialMix.Helpers;

namespace essentialMix.Data.Patterns.Provider;

public abstract class DataProvider<TType> : IDataProvider<TType>
	where TType : struct, IComparable
{
	private IDbConnection _connection;

	protected DataProvider()
	{
	}

	protected DataProvider([NotNull] IDbConnection connection)
	{
		Connection = connection;
	}

	public abstract DbProviderFactory Factory { get; }

	public IDbConnection Connection
	{
		get => _connection ??= Factory.CreateConnection();
		set => _connection = value;
	}

	public abstract TType DefaultDbType { get; }

	public virtual DbCommandBuilder CreateCommandBuilder(IDbCommand selectCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true)
	{
		return Factory.CreateCommandBuilder(selectCommand, missingSchemaAction, missingMappingAction, conflictOption, setAllValues);
	}

	public virtual DbCommandBuilder CreateCommandBuilder(IDbDataAdapter adapter = null, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true)
	{
		return Factory.CreateCommandBuilder(adapter, conflictOption, setAllValues);
	}

	public virtual IDbDataAdapter GetAdapter(IDbCommand selectCommand = null, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return Factory.CreateSelectDataAdapter(selectCommand, missingSchemaAction, missingMappingAction);
	}

	[NotNull]
	public virtual string FormatValue(DataRow row, DataColumn column) { return FormatValue(row[column.ColumnName], column.DataType); }

	[NotNull]
	public virtual string FormatValue(object value, Type type) { return FormatValue(value, MapType(type)); }

	[NotNull]
	public virtual string FormatValue<T>(T value, TType dbType)
	{
		if (value.IsNull()) return "NULL";
		return IsTextual(dbType) ? string.Concat("'", value, "'") : Convert.ToString(value);
	}

	public virtual TType MapType(Type type)
	{
		if (type == null) return DefaultDbType;
		return TypeToTDbType.TryGetValue(type.ResolveType(), out TType dbType) ? dbType : DefaultDbType;
	}

	public virtual Type MapType(TType dbType)
	{
		return TdbTypeToType.TryGetValue(dbType, out Type type)
					? type
					: null;
	}

	public virtual bool IsTextual(TType type) { return TextualTDbType.Contains(type); }

	public virtual bool BuildSchema(DataTable table, IDbTransaction transaction = null)
	{
		table.Clear();
		table.Columns.Clear();
		if (string.IsNullOrEmpty(table.TableName)) throw new ArgumentException("Table's name is empty.", nameof(table));

		DataTable schema = GetSchema(table.TableName, transaction);
		if (!schema.IsValid()) return false;
		table.Merge(schema, false, MissingSchemaAction.AddWithKey);
		return true;
	}

	public virtual bool BuildSchema(string query, DataTable table, IDbTransaction transaction = null)
	{
		table.Clear();
		table.Columns.Clear();
		if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));

		DataTable schema = GetQuerySchema(query, transaction);
		if (!schema.IsValid()) return false;
		table.Merge(schema, false, MissingSchemaAction.AddWithKey);
		return true;
	}

	public virtual DataTable GetSchema(string tableName, IDbTransaction transaction = null)
	{
		if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
		return GetQuerySchema($"SELECT * FROM {tableName} WHERE 0=1", transaction);
	}

	public virtual DataTable GetQuerySchema(string query, IDbTransaction transaction = null)
	{
		if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
		return ExecuteReader(query, CommandType.Text, CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo, transaction)?.GetSchemaTable();
	}

	public virtual DataSet ExecuteQuery(string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
	{
		if (commandText.Length == 0) throw new ArgumentNullException(nameof(commandText));

		bool bMustClose = false;

		if (!Connection.IsOpen())
		{
			Connection.Open();
			bMustClose = Connection.IsValid();
			if (!bMustClose) return null;
		}

		DataSet result = null;
		IDbCommand cmd = null;

		try
		{
			cmd = Connection.CreateCommand(commandText, commandType, transaction);
			IDbDataAdapter adapter = GetAdapter(cmd);

			if (adapter != null)
			{
				result = new DataSet();
				adapter.FillSchema(result, SchemaType.Source);
				adapter.Fill(result);

				if (!result.IsValid())
					ObjectHelper.Dispose(ref result);
			}
		}
		catch
		{
			result = null;
		}
		finally
		{
			ObjectHelper.Dispose(ref cmd);
			if (bMustClose) Connection.Close();
		}

		return result;
	}

	public virtual DataTable ExecuteTable(string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
	{
		DataSet ds = ExecuteQuery(commandText, commandType, transaction);
		return ds.IsValid(true) ? ds.Tables[0] : null;
	}

	public virtual IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction transaction = null)
	{
		if (commandText.Length == 0) throw new ArgumentNullException(nameof(commandText));

		bool bMustClose = false;

		if (!Connection.IsOpen())
		{
			Connection.Open();
			bMustClose = Connection.IsValid();
			if (!bMustClose) return null;
		}

		IDataReader result;

		try
		{
			using (IDbCommand cmd = Connection.CreateCommand(commandText, commandType, transaction))
			{
				result = cmd.ExecuteReader(behavior);
			}
		}
		catch
		{
			result = null;
			if (bMustClose) Connection.Close();
		}

		return result;
	}

	public virtual T ExecuteScalar<T>(string commandText, T defaultValue = default(T), CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
	{
		if (string.IsNullOrWhiteSpace(commandText)) throw new ArgumentNullException(nameof(commandText));

		bool bMustClose = false;

		if (!Connection.IsOpen())
		{
			Connection.Open();
			bMustClose = Connection.IsValid();
			if (!bMustClose) return defaultValue;
		}

		T result;

		try
		{
			using (IDbCommand cmd = Connection.CreateCommand(commandText, commandType, transaction))
				result = cmd.ExecuteScalar().To(defaultValue);
		}
		catch
		{
			result = defaultValue;
		}
		finally
		{
			if (bMustClose) Connection.Close();
		}

		return result;
	}

	public virtual int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
	{
		if (commandText.Length == 0) throw new ArgumentNullException(nameof(commandText));

		bool bMustClose = false;

		if (!Connection.IsOpen())
		{
			Connection.Open();
			bMustClose = Connection.IsValid();
			if (!bMustClose) return -1;
		}

		int result;

		try
		{
			using (IDbCommand cmd = Connection.CreateCommand(commandText, commandType, transaction))
				result = cmd.ExecuteNonQuery();
		}
		catch
		{
			result = -1;
		}
		finally
		{
			if (bMustClose) Connection.Close();
		}

		return result;
	}

	public virtual bool FillTable(DataTable table, string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null)
	{
		table.Clear();
		if (commandText.Length == 0) throw new ArgumentNullException(nameof(commandText));

		IDataReader reader = ExecuteReader(commandText, commandType, CommandBehavior.KeyInfo | CommandBehavior.SequentialAccess, transaction);
		if (!reader.IsValid()) return false;
		table.Load(reader, LoadOption.OverwriteChanges);
		return true;
	}

	[NotNull]
	protected abstract IReadOnlyDictionary<Type, TType> TypeToTDbType { get; }

	[NotNull]
	protected abstract IReadOnlyDictionary<TType, Type> TdbTypeToType { get; }

	[NotNull]
	protected abstract IReadOnlySet<TType> TextualTDbType { get; }
}