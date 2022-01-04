using System;
using essentialMix.Helpers;

namespace essentialMix.Data.Entity;

public abstract class DbMigration : System.Data.Entity.Migrations.DbMigration
{
	protected const string DIR_MIGRATIONS = "Migrations\\";
	protected const string DIR_SQL = "SQL\\";
	protected const string DIR_RELATIVE = "..\\";

	static DbMigration()
	{
		RootPath = PathHelper.AddDirectorySeparator(AppDomain.CurrentDomain.BaseDirectory);
		MigrationsPath = string.Concat(RootPath, DIR_MIGRATIONS);
		BuildMigrationsPath = string.Concat(RootPath, DIR_RELATIVE, DIR_RELATIVE, DIR_MIGRATIONS);
		SQLPath = string.Concat(MigrationsPath, DIR_SQL);
		BuildSQLPath = string.Concat(BuildMigrationsPath, DIR_SQL);
	}

	/// <inheritdoc />
	protected DbMigration()
	{
	}

	protected static string RootPath { get; }
	protected static string MigrationsPath { get; }
	protected static string SQLPath { get; }
	protected static string BuildMigrationsPath { get; }
	protected static string BuildSQLPath { get; }
}