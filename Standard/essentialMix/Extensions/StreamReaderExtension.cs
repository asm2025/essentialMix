using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class StreamReaderExtension
{
	// The index of the next char to be read from charBuffer
	private static readonly Lazy<FieldInfo> __charPos = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("charPos", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);
	// The number of decoded chars presently used in charBuffer
	private static readonly Lazy<FieldInfo> __charLen = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("charLen", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);
	// The current buffer of decoded characters
	private static readonly Lazy<FieldInfo> __charBuffer = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("charBuffer", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);
	private static readonly Lazy<FieldInfo> __bytePos = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("bytePos", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);
	// The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
	private static readonly Lazy<FieldInfo> __byteLen = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("byteLen", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);
	// The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
	private static readonly Lazy<FieldInfo> __byteBuffer = new Lazy<FieldInfo>(() => typeof(StreamReader).GetField("byteBuffer", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly | BindingFlags.GetField), LazyThreadSafetyMode.PublicationOnly);

	public static int Read([NotNull] this StreamReader thisValue, [NotNull] char[] buffer) { return thisValue.Read(buffer, 0, buffer.Length); }

	public static Task<int> ReadAsync([NotNull] this StreamReader thisValue, [NotNull] char[] buffer) { return thisValue.ReadAsync(buffer, 0, buffer.Length); }

	public static int ReadBlock([NotNull] this StreamReader thisValue, [NotNull] char[] buffer) { return thisValue.ReadBlock(buffer, 0, buffer.Length); }

	public static Task<int> ReadBlockAsync([NotNull] this StreamReader thisValue, [NotNull] char[] buffer) { return thisValue.ReadBlockAsync(buffer, 0, buffer.Length); }

	public static int ReadLines([NotNull] this StreamReader thisValue, [NotNull] IList<string> list, int count, int startAtLine = 0)
	{
		list.Clear();
		if (thisValue.EndOfStream) return 0;
		startAtLine = startAtLine.NotBelow(0);

		int result = 0;
		int skip = startAtLine;

		try
		{
			while (skip > 0 && !thisValue.EndOfStream)
			{
				thisValue.ReadLine();
				skip--;
			}

			if (thisValue.EndOfStream) return result;
			result = startAtLine;

			bool useLimit = count > 0;

			while ((!useLimit || list.Count < count) && !thisValue.EndOfStream)
			{
				string line = thisValue.ReadLine();
				result++;
				if (line == null) break;
				if (line.Length == 0) continue;
				list.Add(line);
			}
		}
		catch (IOException)
		{
			result = -1;
			list.Clear();
		}

		return result;
	}

	public static int ReadLines([NotNull] this StreamReader thisValue, [NotNull] ISet<string> set, int count, int startAtLine = 0)
	{
		if (set == null) throw new ArgumentNullException(nameof(set));
		set.Clear();
		if (thisValue.EndOfStream) return 0;
		startAtLine = startAtLine.NotBelow(0);

		int result = 0;
		int skip = startAtLine;

		try
		{
			while (skip > 0 && !thisValue.EndOfStream)
			{
				thisValue.ReadLine();
				skip--;
			}

			if (thisValue.EndOfStream) return result;
			result = startAtLine;

			bool useLimit = count > 0;

			while ((!useLimit || set.Count < count) && !thisValue.EndOfStream)
			{
				string line = thisValue.ReadLine();
				result++;
				if (line == null) break;
				if (line.Length == 0) continue;
				set.Add(line);
			}
		}
		catch (IOException)
		{
			result = -1;
			set.Clear();
		}

		return result;
	}

	public static int ReadAllLines([NotNull] this StreamReader thisValue, [NotNull] IList<string> list) { return ReadLines(thisValue, list, 0); }

	public static int ReadAllLines([NotNull] this StreamReader thisValue, [NotNull] ISet<string> set) { return ReadLines(thisValue, set, 0); }

	public static int CharPos([NotNull] this StreamReader thisValue) { return (int)__charPos.Value.GetValue(thisValue); }

	public static int CharLen([NotNull] this StreamReader thisValue) { return (int)__charLen.Value.GetValue(thisValue); }

	public static char[] CharBuffer([NotNull] this StreamReader thisValue) { return (char[])__charBuffer.Value.GetValue(thisValue); }

	public static int BytePos([NotNull] this StreamReader thisValue) { return (int)__bytePos.Value.GetValue(thisValue); }

	public static int ByteLen([NotNull] this StreamReader thisValue) { return (int)__byteLen.Value.GetValue(thisValue); }

	public static byte[] ByteBuffer([NotNull] this StreamReader thisValue) { return (byte[])__byteBuffer.Value.GetValue(thisValue); }

	/// <summary>
	/// Works for UTF8, UTF-16LE, UTF-16BE, UTF-32LE, UTF-32BE, and any single-byte encoding.
	/// Don't use it with UTF-7, Shift-JIS, or other variable-byte encodings
	/// </summary>
	/// <param name="thisValue">The this value.</param>
	/// <returns></returns>
	public static long Position([NotNull] this StreamReader thisValue)
	{
		int charPos = CharPos(thisValue);
		int charLen = CharLen(thisValue);
		char[] charBuffer = CharBuffer(thisValue);
		int byteLen = CharLen(thisValue);
		byte[] byteBuffer = ByteBuffer(thisValue);
		int numBytesLeft = thisValue.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

		// For variable-byte encodings, deal with partial chars at the end of the buffer
		int numFragments = 0;

		if (byteLen > 0 && !thisValue.CurrentEncoding.IsSingleByte)
		{
			switch (thisValue.CurrentEncoding.CodePage)
			{
				case 65001:
					byte byteCountMask = 0;

					while (byteBuffer[byteLen - numFragments - 1] >> 6 == 2) // if the byte is "10xx xxxx", it's a continuation-byte
						byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask

					if (byteBuffer[byteLen - numFragments - 1] >> 6 == 3) // if the byte is "11xx xxxx", it starts a multi-byte char.
						byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
						
					// see if we found as many bytes as the leading-byte says to expect
					if (numFragments > 1 && byteBuffer[byteLen - numFragments] >> 7 - numFragments == byteCountMask)
						numFragments = 0; // no partial-char in the byte-buffer to account for
					break;
				case 1200:
					if (byteBuffer[byteLen - 1] >= 0xd8) // high-surrogate
						numFragments = 2; // account for the partial character
					break;
				case 1201:
					if (byteBuffer[byteLen - 2] >= 0xd8) // high-surrogate
						numFragments = 2; // account for the partial character
					break;
			}
		}

		return thisValue.BaseStream.Position - numBytesLeft - numFragments;
	}

	public static void Position([NotNull] this StreamReader thisValue, long position)
	{
		thisValue.DiscardBufferedData();
		thisValue.BaseStream.Position = position;
	}

	public static long CountLines([NotNull] this StreamReader thisValue)
	{
		Stream stream = thisValue.BaseStream;
		if (stream.Length == 0) return 0;
		if (!stream.CanRead) throw new IOException("Stream cannot be read.");
		if (!stream.CanSeek) throw new IOException("Stream cannot seek.");

		stream.Position = 0;

		int read;
		long len = 0L;
		char prev = Constants.CHAR_NULL;
		char[] buffer = new char[Constants.BUFFER_MB];
		bool pendingTermination = false;

		while ((read = thisValue.Read(buffer)) > 0)
		{
			for (int i = 0; i < read; i++)
			{
				char cc = buffer[i];
				if (cc == Constants.CHAR_NULL) continue;

				if (cc is Constants.CR or Constants.LF)
				{
					if (prev == Constants.CR && cc == Constants.LF) continue;
					len++;
					pendingTermination = false;
				}
				else
				{
					pendingTermination = true;
				}

				prev = cc;
			}
		}

		if (pendingTermination) len++;
		stream.Position = 0;
		return len;
	}

	public static long CountChar([NotNull] this StreamReader thisValue, char c)
	{
		Stream stream = thisValue.BaseStream;
		if (stream.Length == 0) return 0;
		if (!stream.CanRead) throw new IOException("Stream cannot be read.");
		if (!stream.CanSeek) throw new IOException("Stream cannot seek.");

		stream.Position = 0;

		int read;
		long len = 0L;
		char[] buffer = new char[Constants.BUFFER_MB];

		while ((read = thisValue.Read(buffer)) > 0)
		{
			for (int i = 0; i < read; i++)
			{
				if (c == buffer[i]) len++;
			}
		}

		stream.Position = 0;
		return len;
	}
}