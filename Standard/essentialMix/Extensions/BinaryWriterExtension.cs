using System.IO;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class BinaryWriterExtension
{
	public static void Write([NotNull] this BinaryWriter thisValue, [NotNull] params byte[] buffer)
	{
		thisValue.Write(buffer, 0, buffer.Length);
	}

	public static void Write([NotNull] this BinaryWriter thisValue, [NotNull] params char[] buffer)
	{
		thisValue.Write(buffer, 0, buffer.Length);
	}
}