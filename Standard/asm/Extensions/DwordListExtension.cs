using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class DwordListExtension
	{
		[NotNull]
		public static byte[] AsBytes([NotNull] this DwordList thisValue) { return (byte[])thisValue; }
	}
}