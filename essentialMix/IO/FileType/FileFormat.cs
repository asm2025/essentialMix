using System;
using System.Diagnostics;
using System.IO;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType;

// based on https://github.com/neilharvey/FileSignatures/
[DebuggerDisplay("{Extension}[{MimeType}]")]
public abstract record FileFormat : IEquatable<FileFormat>
{
	protected FileFormat(string extension, string mimeType, byte[] signature)
		: this(extension, mimeType, signature, 0)
	{
	}

	protected FileFormat(string extension, string mimeType, byte[] signature, int offset)
	{
		if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
		Extension = extension?.Trim('.');
		HasWildCard = !string.IsNullOrEmpty(Extension) && (Extension.IndexOf('*') >= 0 || Extension.IndexOf('?') >= 0);
		MimeType = mimeType;
		Signature = signature;
		Offset = offset;
	}

	public string Extension { get; }
	public bool HasWildCard { get; }
	public string MimeType { get; }
	public byte[] Signature { get; }
	public int Offset { get; }

	/// <inheritdoc />
	[NotNull]
	public override string ToString() { return $"{Extension}[{MimeType}]"; }

	/// <inheritdoc />
	public virtual bool Equals(FileFormat other)
	{
		return other != null
				&& Offset == other.Offset
				&& string.Equals(Extension, other.Extension, StringComparison.OrdinalIgnoreCase)
				&& Signature.IsSequenceEqual(other.Signature);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Extension != null
								? Extension.GetHashCode()
								: 0;
			hashCode = (hashCode * 397) ^ HasWildCard.GetHashCode();
			hashCode = (hashCode * 397) ^
						(MimeType != null
							? MimeType.GetHashCode()
							: 0);
			hashCode = Signature != null
							? (hashCode * 397) ^ Signature.GetHashCode()
							: 0;
			hashCode = (hashCode * 397) ^ Offset;
			return hashCode;
		}
	}

	public virtual bool IsMatch([NotNull] Stream stream)
	{
		if (!stream.CanSeek || Signature == null || Signature.Length == 0) throw new NotSupportedException("Stream type is not supported.");
		if (Offset + Signature.Length > stream.Length) return false;
		stream.Position = Offset;
		byte[] buffer = new byte[Signature.Length];
		return stream.Read(buffer, 0, buffer.Length) == buffer.Length && IsMatch(buffer);
	}

	protected virtual bool IsMatch(byte[] buffer)
	{
		return Signature.IsSequenceEqual(buffer);
	}
}