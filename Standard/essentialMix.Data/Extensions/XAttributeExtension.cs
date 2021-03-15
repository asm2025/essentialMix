using System.Runtime.CompilerServices;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class XAttributeExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this XAttribute thisValue) { return Get(thisValue, default(T)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this XAttribute thisValue, T defaultValue)
		{
			return thisValue == null
						? defaultValue
						: thisValue.Value.To(defaultValue);
		}
	}
}