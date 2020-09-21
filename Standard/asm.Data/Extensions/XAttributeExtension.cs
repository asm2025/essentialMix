using System.Xml.Linq;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class XAttributeExtension
	{
		public static T Get<T>([NotNull] this XAttribute thisValue) { return Get(thisValue, default(T)); }

		public static T Get<T>([NotNull] this XAttribute thisValue, T defaultValue) { return thisValue.Value.To(defaultValue); }
	}
}