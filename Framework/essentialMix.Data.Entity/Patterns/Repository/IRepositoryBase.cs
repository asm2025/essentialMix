using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Repository;
using JetBrains.Annotations;
using SystemDbContext = System.Data.Entity.DbContext;

namespace essentialMix.Data.Entity.Patterns.Repository
{
	public interface IRepositoryBase<out TContext, TEntity> : IRepositoryBase<TEntity>
		where TContext : SystemDbContext
		where TEntity : class, IEntity
	{
		[NotNull]
		TContext Context { get; }

		[NotNull]
		DbSet<TEntity> DbSet { get; }

		DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters);
	}
}