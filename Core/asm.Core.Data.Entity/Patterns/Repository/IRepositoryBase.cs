using System.Linq;
using asm.Data.Model;
using asm.Data.Patterns.Repository;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace asm.Core.Data.Entity.Patterns.Repository
{
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
}