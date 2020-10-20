using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.XPath;
using JetBrains.Annotations;
using asm.Data.Xml;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XPathNodeIteratorExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static XmlNodeEnumerator XmlEnumerate([NotNull] this XPathNodeIterator thisValue) { return new XmlNodeEnumerator(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static XmlNodeLister XmlList([NotNull] this XPathNodeIterator thisValue) { return new XmlNodeLister(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static XNodeEnumerator XEnumerate([NotNull] this XPathNodeIterator thisValue) { return new XNodeEnumerator(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static XNodeLister XList([NotNull] this XPathNodeIterator thisValue) { return new XNodeLister(thisValue); }
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static XmlNode GetNode([NotNull] this XPathNodeIterator thisValue)
		{
			return thisValue.Current?.GetNode();
		}
	}
}