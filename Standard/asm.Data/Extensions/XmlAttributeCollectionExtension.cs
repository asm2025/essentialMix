using System.Runtime.CompilerServices;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XmlAttributeCollectionExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Has(this XmlAttributeCollection thisValue, [NotNull] string name)
		{
			return thisValue?[name] != null;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool Has(this XmlAttributeCollection thisValue, [NotNull] string localName, string namespaceURI)
		{
			return thisValue?[localName, namespaceURI] != null;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Attribute([NotNull] this XmlAttributeCollection thisValue, [NotNull] string name)
		{
			return thisValue[name]?.Value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Attribute([NotNull] this XmlAttributeCollection thisValue, [NotNull] string localName, string namespaceURI)
		{
			return thisValue[localName, namespaceURI]?.Value;
		}
	}
}