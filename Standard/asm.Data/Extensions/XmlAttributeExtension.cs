using System.Runtime.CompilerServices;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XmlAttributeExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this XmlAttribute thisValue) { return Get(thisValue, default(T)); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this XmlAttribute thisValue, T defaultValue)
		{
			return thisValue == null
						? defaultValue
						: thisValue.Value.To(defaultValue);
		}
	}
}