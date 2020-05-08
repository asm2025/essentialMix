using System.Data;
using System.Reflection;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class DataColumnCollectionExtension
	{
		public static DataTable GetTable([NotNull] this DataColumnCollection thisValue)
		{
			FieldInfo info = thisValue.AsType().FindField("table", asm.Constants.BF_NON_PUBLIC_INSTANCE, typeof(DataTable));
			return (DataTable)info?.GetValue(thisValue);
		}

		public static bool IsNullOrEmpty(this DataColumnCollection thisValue) { return thisValue == null || thisValue.Count == 0; }
	}
}