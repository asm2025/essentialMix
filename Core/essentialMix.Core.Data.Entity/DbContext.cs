using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace essentialMix.Core.Data.Entity;

public abstract class DbContext : SystemDbContext
{
	/// <inheritdoc />
	protected DbContext()
	{
	}

	/// <inheritdoc />
	protected DbContext([NotNull] DbContextOptions options)
		: base(options)
	{
	}

	/// <inheritdoc />
	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		Validate();
		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	/// <inheritdoc />
	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
	{
		Validate();
		return base.SaveChangesAsync(cancellationToken);
	}

	protected virtual void Validate()
	{
		IEnumerable<EntityEntry> entries = ChangeTracker.Entries()
														.Where(e => e.State is EntityState.Added or EntityState.Modified);

		foreach (EntityEntry entry in entries)
		{
			object entity = entry.Entity;
			Validator.ValidateObject(entity, new ValidationContext(entity), true);
		}
	}
}