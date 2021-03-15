using System.IO;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class TextWriterExtension
	{
		public static void Fprintf([NotNull] this TextWriter thisValue, [NotNull] string format, [NotNull] params object[] parameters)
		{
			thisValue.Write(format.Sprintf(parameters));
		}
	}
}