using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Extensions;
using SystemDbContext = System.Data.Entity.DbContext;

namespace essentialMix.Data.Entity
{
	public abstract class DbContext : SystemDbContext
	{
		/// <inheritdoc />
		protected DbContext()
		{
		}

		/// <inheritdoc />
		protected DbContext(DbCompiledModel model) 
			: base(model)
		{
		}

		/// <inheritdoc />
		protected DbContext(string nameOrConnectionString) 
			: base(nameOrConnectionString)
		{
		}

		/// <inheritdoc />
		protected DbContext(string nameOrConnectionString, DbCompiledModel model) 
			: base(nameOrConnectionString, model)
		{
		}

		/// <inheritdoc />
		protected DbContext(DbConnection existingConnection, bool contextOwnsConnection) 
			: base(existingConnection, contextOwnsConnection)
		{
		}

		/// <inheritdoc />
		protected DbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) 
			: base(existingConnection, model, contextOwnsConnection)
		{
		}

		/// <inheritdoc />
		protected DbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext) 
			: base(objectContext, dbContextOwnsObjectContext)
		{
		}

		/// <inheritdoc />
		public override int SaveChanges()
		{
			try
			{
				return base.SaveChanges();
			}
			catch (DbEntityValidationException e)
			{
				throw new DbEntityValidationException(e.CollectDataMessages(), e.EntityValidationErrors);
			}
		}

		/// <inheritdoc />
		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
		{
			try
			{
				return base.SaveChangesAsync(cancellationToken);
			}
			catch (DbEntityValidationException e)
			{
				throw new DbEntityValidationException(e.CollectDataMessages(), e.EntityValidationErrors);
			}
		}
	}
}