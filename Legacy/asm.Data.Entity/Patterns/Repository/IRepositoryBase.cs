using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using asm.Data.Model;
using asm.Data.Patterns.Repository;
using JetBrains.Annotations;
using SystemDbContext = System.Data.Entity.DbContext;

namespace asm.Data.Entity.Patterns.Repository
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