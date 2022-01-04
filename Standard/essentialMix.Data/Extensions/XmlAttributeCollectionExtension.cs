using System.Runtime.CompilerServices;
using System.Xml;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

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
	public static string Get([NotNull] this XmlAttributeCollection thisValue, [NotNull] string name)
	{
		return thisValue[name]?.Value;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static string Get([NotNull] this XmlAttributeCollection thisValue, [NotNull] string localName, string namespaceURI)
	{
		return thisValue[localName, namespaceURI]?.Value;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static T Get<T>([NotNull] this XmlAttributeCollection thisValue, [NotNull] string name, T defaultValue)
	{
		XmlAttribute attribute = thisValue[name];
		return attribute != null
					? attribute.Value.To(defaultValue)
					: defaultValue;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static T Get<T>([NotNull] this XmlAttributeCollection thisValue, [NotNull] string localName, string namespaceURI, T defaultValue)
	{
		XmlAttribute attribute = thisValue[localName, namespaceURI];
		return attribute != null
					? attribute.Value.To(defaultValue)
					: defaultValue;
	}
}