using System.IO;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class BinaryReaderExtension
{
	public static int Read([NotNull] this BinaryReader thisValue, [NotNull] byte[] buffer) { return thisValue.Read(buffer, 0, buffer.Length); }

	public static int Read([NotNull] this BinaryReader thisValue, [NotNull] char[] buffer) { return thisValue.Read(buffer, 0, buffer.Length); }

	public static bool EndOfStream([NotNull] this BinaryReader thisValue) { return thisValue.BaseStream.Position == thisValue.BaseStream.Length; }
}