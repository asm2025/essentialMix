using System.Linq;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Repository;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace essentialMix.Core.Data.Entity.Patterns.Repository;

public interface IRepositoryBase<out TContext, TEntity> : IRepositoryBase<TEntity>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	[NotNull]
	TContext Context { get; }

	[NotNull]
	DbSet<TEntity> DbSet { get; }

	IQueryable<TEntity> SqlQuery(string sql, params object[] parameters);
}