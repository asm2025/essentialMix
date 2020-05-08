using System.Xml.Linq;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Data.Extensions
{
	public static class XAttributeExtension
	{
		public static T Get<T>([NotNull] this XAttribute thisValue) { return Get(thisValue, default(T)); }

		public static T Get<T>([NotNull] this XAttribute thisValue, T defaultValue) { return thisValue.Value.To(defaultValue); }
	}
}