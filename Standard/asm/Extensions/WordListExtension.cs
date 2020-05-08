using JetBrains.Annotations;
using asm.Collections;

namespace asm.Extensions
{
	public static class WordListExtension
	{
		[NotNull] public static byte[] AsBytes([NotNull] this WordList thisValue) { return (byte[])thisValue; }
	}
}