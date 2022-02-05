using System.Data;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DataRowCollectionExtension
{
	public static DataTable GetTable([NotNull] this DataRowCollection thisValue)
	{
		FieldInfo info = thisValue.AsType().FindField("table", Constants.BF_NON_PUBLIC_INSTANCE, typeof(DataTable));
		return (DataTable)info?.GetValue(thisValue);
	}

	public static bool IsNullOrEmpty(this DataRowCollection thisValue) { return thisValue == null || thisValue.Count == 0; }
}