using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;
using essentialMix.Helpers;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class IDbConnectionExtension
	{
		[NotNull]
		public static DbProviderFactory GetFactory([NotNull] this IDbConnection thisValue)
		{
			Type type = thisValue.GetType();
			DbProviderFactory result;


			PropertyInfo property = type.GetProperty("DbProviderFactory", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.GetProperty);

			if (property != null && property.PropertyType.IsAssignableTo<DbProviderFactory>())
			{
				result = (DbProviderFactory)property.GetValue(thisValue);
			}
			else
			{
				// next, try to get the static instance of the factory. This won't work if the config (web.config or app.config) is missing this section
				Type providerType = TypeHelper.FromName(type.FullName, type.AssemblyQualifiedName);
				result = (DbProviderFactory)providerType?.InvokeMember("Instance", Constants.BF_PUBLIC_STATIC | BindingFlags.GetProperty | BindingFlags.GetField, null, type, null);
			}

			return result ?? throw new InvalidOperationException("Database Provider not found.");
		}

		public static bool IsValid(this IDbConnection thisValue) { return thisValue != null && (IsOpen(thisValue) || IsBusy(thisValue)); }

		public static bool IsOpen([NotNull] this IDbConnection thisValue) { return thisValue.State.HasFlag(ConnectionState.Open); }

		public static bool IsBusy([NotNull] this IDbConnection thisValue)
		{
			return thisValue.State.HasFlag(ConnectionState.Executing) | thisValue.State.HasFlag(ConnectionState.Fetching);
		}

		[NotNull]
		public static IDbCommand CreateCommand([NotNull] this IDbConnection thisValue, string commandText, CommandType type = CommandType.Text, IDbTransaction transaction = null)
		{
			IDbCommand cmd = thisValue.CreateCommand();
			cmd.CommandText = commandText;
			cmd.CommandType = type;
			cmd.Transaction = transaction;
			return cmd;
		}
	}
}