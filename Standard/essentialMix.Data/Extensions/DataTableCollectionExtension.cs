using System;
using System.Data;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class DataTableCollectionExtension
	{
		public static void Remove([NotNull] this DataTableCollection thisValue, [NotNull] params string[] names)
		{
			if (thisValue.Count == 0 || names.Length == 0) return;

			foreach (string name in names)
				thisValue.Remove(name);
		}

		public static void Remove([NotNull] this DataTableCollection thisValue, [NotNull] Predicate<DataTable> selector)
		{
			if (thisValue.Count == 0) return;

			string[] names = thisValue.Cast<DataTable>().Where(t => selector(t)).Select(t => t.TableName).ToArray();
			Remove(thisValue, names);
		}
	}
}