using System;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Data.Entity.Patterns.Provider
{
	public interface IDataProvider<TType> : Data.Patterns.Provider.IDataProvider<TType>
		where TType : struct, IComparable
	{
		bool BuildSchema([NotNull] IQueryable query, [NotNull] DataTable table, IDbTransaction transaction = null);
		DataTable GetQuerySchema([NotNull] IQueryable query, IDbTransaction transaction = null);
		DataSet ExecuteQuery([NotNull] IQueryable query, IDbTransaction transaction = null);
		DataTable ExecuteTable([NotNull] IQueryable query, IDbTransaction transaction = null);
		IDataReader ExecuteReader([NotNull] IQueryable query, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction transaction = null);
		T ExecuteScalar<T>([NotNull] IQueryable query, T defaultValue = default(T), IDbTransaction transaction = null);
		int ExecuteNonQuery([NotNull] IQueryable query, IDbTransaction transaction = null);
		bool FillTable([NotNull] DataTable table, [NotNull] IQueryable query, IDbTransaction transaction = null);
		[NotNull]
		string GetQueryStatement([NotNull] IQueryable query);
		[NotNull]
		string GetQueryStatement<T>([NotNull] IQueryable<T> query);
		[NotNull]
		string GetQueryDebugStatement([NotNull] IQueryable query);
		[NotNull]
		string GetQueryDebugStatement<T>([NotNull] IQueryable<T> query);
	}
}
