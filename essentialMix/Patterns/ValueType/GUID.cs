using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace essentialMix.Patterns.ValueType;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GUID
{
	private const int LAST_BYTES_LEN = 8;
	
	private byte[] _data4;
	
	public uint Data1 { get; set; }
	public ushort Data2 { get; set; }
	public ushort Data3 { get; set; }
	[NotNull]
	public byte[] Data4 
	{
		get => _data4 ??= new byte[LAST_BYTES_LEN];
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value.Length != LAST_BYTES_LEN) throw new ArgumentNullException(nameof(value));
			_data4 = value;
		} 
	}

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