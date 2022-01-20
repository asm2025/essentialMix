using System.Linq;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Repository;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace essentialMix.Core.Data.Entity.Patterns.Repository;

public interface IRepositoryBase<TContext, TEntity> : IRepositoryBase<TEntity>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	[NotNull]
	TContext Context { get; }

	[NotNull]
	DbSet<TEntity> DbSet { get; }

	IQueryable<TEntity> SqlQuery(string sql, params object[] parameters);
}

public interface IRepositoryBase<TContext, TEntity, TKey> : IRepositoryBase<TContext, TEntity>, essentialMix.Data.Patterns.Repository.IRepositoryBase<TEntity, TKey>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepositoryBase<TContext, TEntity, TKey1, TKey2> : IRepositoryBase<TContext, TEntity>, essentialMix.Data.Patterns.Repository.IRepositoryBase<TEntity, TKey1, TKey2>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3> : IRepositoryBase<TContext, TEntity>, essentialMix.Data.Patterns.Repository.IRepositoryBase<TEntity, TKey1, TKey2, TKey3>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4> : IRepositoryBase<TContext, TEntity>, essentialMix.Data.Patterns.Repository.IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IRepositoryBase<TContext, TEntity>, essentialMix.Data.Patterns.Repository.IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}
