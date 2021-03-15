using System.Data;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class IDataParameterCollectionExtension
	{
		public static bool IsNullOrEmpty(this IDataParameterCollection thisValue) { return thisValue == null || thisValue.Count == 0; }
	}
}