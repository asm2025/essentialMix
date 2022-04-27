using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public abstract record IsoSubEbeBase : FileFormatBase
{
	private static readonly byte[] __signature = { 0x93, 0x42, 0x82, 0x88 };

	/// <inheritdoc />
	protected IsoSubEbeBase(string extension, string mimeType, [NotNull] byte[] prefix)
		: this(extension, mimeType, prefix, null, 0)
	{
	}

	/// <inheritdoc />
	protected IsoSubEbeBase(string extension, string mimeType, [NotNull] byte[] prefix, int offset)
		: this(extension, mimeType, prefix, null, offset)
	{
	}

	/// <inheritdoc />
	protected IsoSubEbeBase(string extension, string mimeType, [NotNull] byte[] prefix, [NotNull] byte[] suffix)
		: this(extension, mimeType, prefix, suffix, 0)
	{
	}

	/// <inheritdoc />
	protected IsoSubEbeBase(string extension, string mimeType, byte[] prefix, byte[] suffix, int offset)
		: base(extension, mimeType, prefix.DefaultIfNull().Concat(__signature).Concat(suffix.DefaultIfNull()).ToArray(), offset)
	{
	}
}