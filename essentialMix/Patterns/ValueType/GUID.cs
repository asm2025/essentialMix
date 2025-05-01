using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Patterns.ValueType;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GUID()
{
	private const int LAST_BYTES_LEN = 8;

	public uint Data1 { get; set; } = 0u;
	public ushort Data2 { get; set; } = 0;
	public ushort Data3 { get; set; } = 0;

	[NotNull]
	public byte[] Data4 { get; } = new byte[LAST_BYTES_LEN];

	[NotNull]
	public override string ToString()
	{
		return $"{Data1:x2}-{Data2:x2}-{Data3:x2}-{string.Concat(Data4.Take(2).Select(e => e.ToString("x2")))}-{string.Concat(Data4.Skip(2).Select(e => e.ToString("x2")))}";
	}

	[NotNull]
	public string ToString(bool ignoreEndianness)
	{
		if (!ignoreEndianness) return ToString();
		StringBuilder sb = new StringBuilder();
		sb.Append(string.Concat(BitConverter.GetBytes(Data1).Select(e => e.ToString("x2"))));
		sb.Append('-');
		sb.Append(string.Concat(BitConverter.GetBytes(Data2).Select(e => e.ToString("x2"))));
		sb.Append('-');
		sb.Append(string.Concat(BitConverter.GetBytes(Data3).Select(e => e.ToString("x2"))));
		sb.Append('-');
		sb.Append(string.Concat(Data4.Take(2).Select(e => e.ToString("x2"))));
		sb.Append('-');
		sb.Append(string.Concat(Data4.Skip(2).Select(e => e.ToString("x2"))));
		return sb.ToString();
	}

	public static GUID FromGuid(Guid value)
	{
		byte[] bytes = value.ToByteArray();
		int position = 0;
		GUID guid = new GUID
		{
			Data1 = BitConverter.ToUInt32(bytes, position)
		};
		position += sizeof(uint);
		guid.Data2 = BitConverter.ToUInt16(bytes, position);
		position += sizeof(ushort);
		guid.Data3 = BitConverter.ToUInt16(bytes, position);
		position += sizeof(ushort);
		Buffer.BlockCopy(bytes, position, guid.Data4, 0, LAST_BYTES_LEN);
		return guid;
	}
}