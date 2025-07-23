using System.Data;
using System.Data.Common;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DbProviderFactoryExtension
{
	private const string FMT_PARAMETER_PLACEHOLDER_DEFAULT = "@{0}";

	private static MethodInfo __getParameterNameMethod;
	private static MethodInfo __getParameterPlaceholderMethod;

	private static MethodInfo GetParameterNameMethod
	{
		get
		{
			if (__getParameterNameMethod != null) return __getParameterNameMethod;
			__getParameterNameMethod = typeof(DbCommandBuilder).FindMethod("GetParameterName", Constants.BF_NON_PUBLIC_INSTANCE, null, typeof(string), CallingConventions.Any, null, typeof(string));
			return __getParameterNameMethod;
		}
	}

	private static MethodInfo GetParameterPlaceholderMethod
	{
		get
		{
			if (__getParameterPlaceholderMethod != null) return __getParameterPlaceholderMethod;
			__getParameterPlaceholderMethod = typeof(DbCommandBuilder).FindMethod("GetParameterPlaceholder", Constants.BF_NON_PUBLIC_INSTANCE, null, typeof(string), CallingConventions.Any, null, typeof(int));
			return __getParameterPlaceholderMethod;
		}
	}

	public static IDbDataAdapter CreateSelectDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter<IDbDataAdapter>(thisValue, command, null, null, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateInsertDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, null, command, null, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateUpdateDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, null, null, command, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateDeleteDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, null, null, null, command, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, selectCommand, null, null, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, selectCommand, insertCommand, null, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, IDbCommand updateCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter(thisValue, selectCommand, insertCommand, updateCommand, null, missingSchemaAction, missingMappingAction);
	}

	public static IDbDataAdapter CreateDataAdapter([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, IDbCommand updateCommand, IDbCommand deleteCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
	{
		return CreateDataAdapter<IDbDataAdapter>(thisValue, selectCommand, insertCommand, updateCommand, deleteCommand, missingSchemaAction, missingMappingAction);
	}

	public static T CreateSelectDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, command, null, null, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateInsertDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, null, command, null, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateUpdateDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, null, null, command, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateDeleteDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand command, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, null, null, null, command, missingSchemaAction, missingMappingAction);
	}

	public static T CreateDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, selectCommand, null, null, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, selectCommand, insertCommand, null, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, IDbCommand updateCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		return CreateDataAdapter<T>(thisValue, selectCommand, insertCommand, updateCommand, null, missingSchemaAction, missingMappingAction);
	}

	public static T CreateDataAdapter<T>([NotNull] this DbProviderFactory thisValue, IDbCommand selectCommand, IDbCommand insertCommand, IDbCommand updateCommand, IDbCommand deleteCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough)
		where T : IDbDataAdapter
	{
		T adapter = (T)(IDbDataAdapter)thisValue.CreateDataAdapter();
		if (adapter == null) return default(T);
		adapter.MissingSchemaAction = missingSchemaAction;
		adapter.MissingMappingAction = missingMappingAction;
		adapter.SelectCommand = selectCommand;
		adapter.InsertCommand = insertCommand;
		adapter.UpdateCommand = updateCommand;
		adapter.DeleteCommand = deleteCommand;
		return adapter;
	}

	public static DbCommandBuilder CreateCommandBuilder([NotNull] this DbProviderFactory thisValue, [NotNull] IDbCommand selectCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true)
	{
		IDbDataAdapter adapter = CreateSelectDataAdapter(thisValue, selectCommand, missingSchemaAction, missingMappingAction);
		return adapter == null ? null : CreateCommandBuilder(thisValue, adapter, conflictOption, setAllValues);
	}

	public static DbCommandBuilder CreateCommandBuilder([NotNull] this DbProviderFactory thisValue, IDbDataAdapter adapter = null, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true)
	{
		DbCommandBuilder builder = thisValue.CreateCommandBuilder();
		if (builder == null) return null;
		builder.DataAdapter = (DbDataAdapter)adapter;
		builder.RefreshSchema();
		builder.ConflictOption = conflictOption;
		builder.SetAllValues = setAllValues;
		return builder;
	}

	public static bool GetParameterFormats([NotNull] this DbProviderFactory thisValue, [NotNull] out string nameFormat, [NotNull] out string placeholderFormat)
	{
		nameFormat = FMT_PARAMETER_PLACEHOLDER_DEFAULT;
		placeholderFormat = FMT_PARAMETER_PLACEHOLDER_DEFAULT;

		DbCommandBuilder builder = thisValue.CreateCommandBuilder();
		if (builder == null) return false;
		builder.RefreshSchema();

		string value = (string)GetParameterNameMethod.Invoke(builder, ["t"]);
		if (string.IsNullOrEmpty(value)) return false;
		nameFormat = value.Length == 1 ? "{0}" : value.Left(value.Length - 1) + "{0}";

		value = (string)GetParameterPlaceholderMethod.Invoke(builder, [0]);

		if (string.IsNullOrEmpty(value))
		{
			nameFormat = FMT_PARAMETER_PLACEHOLDER_DEFAULT;
			return false;
		}

		placeholderFormat = value.Length < 3 ? "{0}" : value.Left(value.Length - 2) + "{0}";
		return true;
	}
}