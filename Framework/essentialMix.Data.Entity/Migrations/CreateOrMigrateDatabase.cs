using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.Data.Entity.Migrations
{
	public class CreateOrMigrateDatabase<TContext, TConfiguration> : CreateDatabaseIfNotExists<TContext>, IDatabaseInitializer<TContext>
		where TContext : DbContext
		where TConfiguration : DbMigrationsConfiguration<TContext>, new()
	{
		private readonly TConfiguration _configuration;

		public CreateOrMigrateDatabase()
			: this(new TConfiguration())
		{
		}

		public CreateOrMigrateDatabase([NotNull] string connectionName)
			: this(new TConfiguration { TargetDatabase = new DbConnectionInfo(connectionName) })
		{
		}

		public CreateOrMigrateDatabase([NotNull] string connectionString, [NotNull] string providerInvariantName)
			: this(new TConfiguration { TargetDatabase = new DbConnectionInfo(connectionString, providerInvariantName) })
		{
		}

		public CreateOrMigrateDatabase([NotNull] TConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		/// <inheritdoc />
		public override void InitializeDatabase([NotNull] TContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			bool exists = context.Database.Exists();

			if (exists)
			{
				DbMigrator migrator = new DbMigrator(_configuration);
				if (!context.Database.CompatibleWithModel(false) || migrator.GetPendingMigrations().Any()) migrator.Update();
			}

			base.InitializeDatabase(context);
			if (!exists) return;
			Seed(context);
			context.SaveChanges();
		}
	}
}