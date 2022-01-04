using System;
using System.Text;
using essentialMix.Collections;
using JetBrains.Annotations;
using essentialMix.Helpers;
using essentialMix.Numeric;

namespace essentialMix.Cryptography.Encoders;

public sealed class ByteEncoder : IEncode
{
	private ByteEncoder(BitVectorMode mode, [NotNull] Encoding encoding)
	{
		Mode = mode;
		Encoding = encoding;
	}

	public BitVectorMode Mode { get; }
	public Encoding Encoding { get; set; }

	[NotNull]
	public byte[] EncodeToBytes(string value)
	{
		return string.IsNullOrEmpty(value)
					? Array.Empty<byte>()
					: Encoding.GetBytes(value);
	}

	public string Encode(string value)
	{
		return string.IsNullOrEmpty(value)
					? value
					: Encode(Encoding.GetBytes(value));
	}

	[NotNull]
	public string Encode(byte[] buffer) { return Encode(buffer, 0, buffer.Length); }

	[NotNull]
	public string Encode(byte[] buffer, int startIndex, int count)
	{
		buffer = ArrayHelper.ValidateAndGetRange(buffer, ref startIndex, ref count);
		if (buffer.Length == 0 || count == 0) return string.Empty;
		return new BitVectorList(buffer) { Mode = Mode }.ToString();
	}

	[NotNull]
	public byte[] DecodeToBytes(string value)
	{
		return string.IsNullOrEmpty(value)
					? Array.Empty<byte>()
					: (byte[])new BitVectorList(Mode)
					{
						value
					};
	}

	public string Decode(string value)
	{
		return string.IsNullOrEmpty(value)
					? value
					: Encoding.GetString(DecodeToBytes(value));
	}

	[NotNull]
	public static ByteEncoder Create(BitVectorMode mode, Encoding encoding = null) { return new ByteEncoder(mode, encoding ?? EncodingHelper.Default); }
}