using System;
using essentialMix.Helpers;

namespace essentialMix.Extensions
{
	public static class TypeCodeExtension
	{
		public static bool IsFloater(this TypeCode thisValue) { return ValueTypeHelper.FloatingTypes.Contains(thisValue); }

		public static bool IsIntegral(this TypeCode thisValue) { return ValueTypeHelper.IntegralTypes.Contains(thisValue); }

		public static bool IsNumeric(this TypeCode thisValue) { return ValueTypeHelper.NumericTypes.Contains(thisValue); }
	}
}