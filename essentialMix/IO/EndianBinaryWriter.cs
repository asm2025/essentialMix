using System;
using System.IO;
using System.Text;
using essentialMix.Helpers;
using Other.JonSkeet.MiscUtil.Text;
using essentialMix.Patterns.Object;
using JetBrains.Annotations;

namespace essentialMix.IO;

/// <summary>
/// Equivalent of System.IO.BinaryWriter, but with either endianness, depending on
/// the EndianBitConverter it is constructed with.
/// </summary>
public class EndianBinaryWriter : Disposable
{
	/// <summary>
	/// Buffer used for temporary storage during conversion from primitives
	/// </summary>
	private readonly byte[] _buffer = new byte[16];

	/// <summary>
	/// Buffer used for Write(char)
	/// </summary>
	private readonly char[] _charBuffer = new char[1];
	private Stream _baseStream;

	/// <summary>
	/// Constructs a new binary writer with the given bit converter, writing
	/// to the given stream, using UTF-8 encoding.
	/// </summary>
	/// <param name="bitConverter">Converter to use when writing data</param>
	/// <param name="stream">Stream to write data to</param>
	public EndianBinaryWriter([NotNull] EndianBitConverter bitConverter, [NotNull] Stream stream) 
		: this(bitConverter, stream, EncodingHelper.Default)
	{
	}

	/// <summary>
	/// Constructs a new binary writer with the given bit converter, writing
	/// to the given stream, using the given encoding.
	/// </summary>
	/// <param name="bitConverter">Converter to use when writing data</param>
	/// <param name="stream">Stream to write data to</param>
	/// <param name="encoding">Encoding to use when writing character data</param>
	public EndianBinaryWriter([NotNull] EndianBitConverter bitConverter, [NotNull] Stream stream, [NotNull] Encoding encoding)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		if (!stream.CanWrite) throw new ArgumentException("Stream is not writable", nameof(stream));
		_baseStream = stream;
		BitConverter = bitConverter ?? throw new ArgumentNullException(nameof(bitConverter));
		Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
	}

	/// <summary>
	/// The encoding used to write strings
	/// </summary>
	public virtual Encoding Encoding { get; }

	/// <summary>
	/// The bit converter used to write values to the stream
	/// </summary>
	public EndianBitConverter BitConverter { get; }

	/// <summary>
	/// Gets the underlying stream of the EndianBinaryWriter.
	/// </summary>
	public Stream BaseStream => _baseStream;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Flush();
			ObjectHelper.Dispose(ref _baseStream);
		}
		base.Dispose(disposing);
	}

	/// <summary>
	/// Closes the writer, including the underlying stream.
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	/// <summary>
	/// Flushes the underlying stream.
	/// </summary>
	public void Flush()
	{
		ThrowIfDisposed();
		BaseStream.Flush();
	}

	/// <summary>
	/// Seeks within the stream.
	/// </summary>
	/// <param name="offset">Offset to seek to.</param>
	/// <param name="origin">Origin of seek operation.</param>
	public void Seek(int offset, SeekOrigin origin)
	{
		ThrowIfDisposed();
		BaseStream.Seek(offset, origin);
	}

	/// <summary>
	/// Writes a boolean value to the stream. 1 byte is written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(bool value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 1);
	}

	/// <summary>
	/// Writes a 16-bit signed integer to the stream, using the bit converter
	/// for this writer. 2 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(short value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 2);
	}

	/// <summary>
	/// Writes a 32-bit signed integer to the stream, using the bit converter
	/// for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(int value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 4);
	}

	/// <summary>
	/// Writes a 64-bit signed integer to the stream, using the bit converter
	/// for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(long value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 8);
	}

	/// <summary>
	/// Writes a 16-bit unsigned integer to the stream, using the bit converter
	/// for this writer. 2 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(ushort value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 2);
	}

	/// <summary>
	/// Writes a 32-bit unsigned integer to the stream, using the bit converter
	/// for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(uint value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 4);
	}

	/// <summary>
	/// Writes a 64-bit unsigned integer to the stream, using the bit converter
	/// for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(ulong value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 8);
	}

	/// <summary>
	/// Writes a single-precision floating-point value to the stream, using the bit converter
	/// for this writer. 4 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(float value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 4);
	}

	/// <summary>
	/// Writes a double-precision floating-point value to the stream, using the bit converter
	/// for this writer. 8 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(double value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 8);
	}

	/// <summary>
	/// Writes a decimal value to the stream, using the bit converter for this writer.
	/// 16 bytes are written.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(decimal value)
	{
		BitConverter.CopyBytes(value, _buffer, 0);
		WriteInternal(_buffer, 16);
	}

	/// <summary>
	/// Writes a signed byte to the stream.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(byte value)
	{
		_buffer[0] = value;
		WriteInternal(_buffer, 1);
	}

	/// <summary>
	/// Writes an unsigned byte to the stream.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(sbyte value)
	{
		_buffer[0] = unchecked((byte)value);
		WriteInternal(_buffer, 1);
	}

	/// <summary>
	/// Writes an array of bytes to the stream.
	/// </summary>
	/// <param name="value">The values to write</param>
	public void Write([NotNull] byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}
		WriteInternal(value, value.Length);
	}

	/// <summary>
	/// Writes a portion of an array of bytes to the stream.
	/// </summary>
	/// <param name="value">An array containing the bytes to write</param>
	/// <param name="offset">The index of the first byte to write within the array</param>
	/// <param name="count">The number of bytes to write</param>
	public void Write([NotNull] byte[] value, int offset, int count)
	{
		ThrowIfDisposed();
		BaseStream.Write(value, offset, count);
	}

	/// <summary>
	/// Writes a single character to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">The value to write</param>
	public void Write(char value)
	{
		_charBuffer[0] = value;
		Write(_charBuffer);
	}

	/// <summary>
	/// Writes an array of characters to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">An array containing the characters to write</param>
	public void Write([NotNull] char[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}
		ThrowIfDisposed();
		byte[] data = Encoding.GetBytes(value, 0, value.Length);
		WriteInternal(data, data.Length);
	}

	/// <summary>
	/// Writes a string to the stream, using the encoding for this writer.
	/// </summary>
	/// <param name="value">The value to write. Must not be null.</param>
	/// <exception cref="ArgumentNullException">value == null</exception>
	public void Write([NotNull] string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value));
		}
		ThrowIfDisposed();
		byte[] data = Encoding.GetBytes(value);
		Write7BitEncodedInt(data.Length);
		WriteInternal(data, data.Length);
	}

	/// <summary>
	/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant
	/// information first, with 7 bits of information per byte of value, and the top
	/// bit as a continuation flag.
	/// </summary>
	/// <param name="value">The 7-bit encoded integer to write to the stream</param>
	public void Write7BitEncodedInt(int value)
	{
		ThrowIfDisposed();
		if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to 0.");

		int index = 0;

		while (value >= 128)
		{
			_buffer[index++] = (byte)((value & 0x7f) | 0x80);
			value = value >> 7;
			index++;
		}

		_buffer[index++] = (byte)value;
		BaseStream.Write(_buffer, 0, index);
	}

	/// <summary>
	/// Writes the specified number of bytes from the start of the given byte array,
	/// after checking whether or not the writer has been disposed.
	/// </summary>
	/// <param name="bytes">The array of bytes to write from</param>
	/// <param name="length">The number of bytes to write</param>
	private void WriteInternal([NotNull] byte[] bytes, int length)
	{
		BaseStream.Write(bytes, 0, length);
	}
}