using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Provider
{
	public interface IDataProvider<TType>
		where TType : struct, IComparable
	{
		[NotNull]
		DbProviderFactory Factory { get; }
		IDbConnection Connection { get; set; }
		TType DefaultDbType { get; }
		IDbDataAdapter GetAdapter(IDbCommand selectCommand = null, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough);
		DbCommandBuilder CreateCommandBuilder([NotNull] IDbCommand selectCommand, MissingSchemaAction missingSchemaAction = MissingSchemaAction.AddWithKey, MissingMappingAction missingMappingAction = MissingMappingAction.Passthrough, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true);
		DbCommandBuilder CreateCommandBuilder(IDbDataAdapter adapter = null, ConflictOption conflictOption = ConflictOption.OverwriteChanges, bool setAllValues = true);
		string FormatValue([NotNull] DataRow row, [NotNull] DataColumn column);
		string FormatValue(object value, [NotNull] Type type);
		string FormatValue<T>(T value, TType dbType);
		Type MapType(TType value);
		TType MapType(Type type);
		bool IsTextual(TType value);
		bool BuildSchema([NotNull] DataTable table, IDbTransaction transaction = null);
		bool BuildSchema([NotNull] string query, [NotNull] DataTable table, IDbTransaction transaction = null);
		DataTable GetSchema([NotNull] string tableName, IDbTransaction transaction = null);
		DataTable GetQuerySchema([NotNull] string query, IDbTransaction transaction = null);
		DataSet ExecuteQuery([NotNull] string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null);
		DataTable ExecuteTable([NotNull] string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null);
		IDataReader ExecuteReader([NotNull] string commandText, CommandType commandType = CommandType.Text, CommandBehavior behavior = CommandBehavior.Default, IDbTransaction transaction = null);
		T ExecuteScalar<T>([NotNull] string commandText, T defaultValue = default(T), CommandType commandType = CommandType.Text, IDbTransaction transaction = null);
		int ExecuteNonQuery([NotNull] string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null);
		bool FillTable([NotNull] DataTable table, [NotNull] string commandText, CommandType commandType = CommandType.Text, IDbTransaction transaction = null);
	}
}
