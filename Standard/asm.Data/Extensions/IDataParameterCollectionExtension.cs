using System.Data;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class IDataParameterCollectionExtension
	{
		public static bool IsNullOrEmpty(this IDataParameterCollection thisValue) { return thisValue == null || thisValue.Count == 0; }
	}
}