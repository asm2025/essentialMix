using System.Data;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IDataParameterCollectionExtension
{
	public static bool IsNullOrEmpty(this IDataParameterCollection thisValue) { return thisValue is not { Count: not 0 }; }
}