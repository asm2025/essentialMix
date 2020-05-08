using System.Xml;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class XmlAttributeExtension
	{
		public static T Get<T>([NotNull] this XmlAttribute thisValue) { return Get(thisValue, default(T)); }

		public static T Get<T>([NotNull] this XmlAttribute thisValue, T defaultValue) { return thisValue.Value.To(defaultValue); }
	}
}