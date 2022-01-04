using System;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Core.Data.Entity.Patterns.Provider;

public abstract class DataProvider<TType> : essentialMix.Data.Patterns.Provider.DataProvider<TType>, IDataProvider<TType>
	where TType : struct, IComparable
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

	/// <inheritdoc />
	public bool BuildSchema(IQueryable query, DataTable table, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return BuildSchema(q, table, transaction);
	}

	/// <inheritdoc />
	public DataTable GetQuerySchema(IQueryable query, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return GetQuerySchema(q, transaction);
	}

	/// <inheritdoc />
	public DataSet ExecuteQuery(IQueryable query, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return ExecuteQuery(q, CommandType.Text, transaction);
	}

	/// <inheritdoc />
	public DataTable ExecuteTable(IQueryable query, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return ExecuteTable(q, CommandType.Text, transaction);
	}

	/// <inheritdoc />
	public IDataReader ExecuteReader(IQueryable query, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return ExecuteReader(q, CommandType.Text, behavior, transaction);
	}

	/// <inheritdoc />
	public T ExecuteScalar<T>(IQueryable query, T defaultValue = default(T), IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return ExecuteScalar(q, defaultValue, CommandType.Text, transaction);
	}

	/// <inheritdoc />
	public int ExecuteNonQuery(IQueryable query, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return ExecuteNonQuery(q, CommandType.Text, transaction);
	}

	/// <inheritdoc />
	public bool FillTable(DataTable table, IQueryable query, IDbTransaction transaction = null)
	{
		string q = GetQueryStatement(query);
		return FillTable(table, q, CommandType.Text, transaction);
	}

	/// <inheritdoc />
	public virtual string GetQueryStatement(IQueryable query) { return query.ToString() ?? string.Empty; }

	/// <inheritdoc />
	public virtual string GetQueryStatement<T>(IQueryable<T> query) { return query.ToString() ?? string.Empty; }

	/// <inheritdoc />
	public virtual string GetQueryDebugStatement(IQueryable query) { return GetQueryStatement(query); }

	/// <inheritdoc />
	public virtual string GetQueryDebugStatement<T>(IQueryable<T> query) { return GetQueryStatement(query); }
}