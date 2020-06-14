using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Helpers;

namespace asm.Extensions
{
	public static class StreamExtension
	{
		public static int Read([NotNull] this Stream thisValue, [NotNull] byte[] buffer) { return thisValue.Read(buffer, 0, buffer.Length); }

		public static Task<int> ReadAsync([NotNull] this Stream thisValue, [NotNull] byte[] buffer, CancellationToken token = default(CancellationToken))
		{
			return thisValue.ReadAsync(buffer, 0, buffer.Length, token);
		}

		public static void Write([NotNull] this Stream thisValue, [NotNull] byte[] buffer)
		{
			thisValue.Write(buffer, 0, buffer.Length);
		}

		public static Task WriteAsync([NotNull] this Stream thisValue, [NotNull] byte[] buffer, CancellationToken token = default(CancellationToken))
		{
			return thisValue.WriteAsync(buffer, 0, buffer.Length, token);
		}

		public static long CopyTo([NotNull] this Stream thisValue, Stream output, int bufferSize = -1)
		{
			if (bufferSize == -1) bufferSize = Constants.GetBufferKB(100);
			if (bufferSize < StreamHelper.BUFFER_DEFAULT) bufferSize = StreamHelper.BUFFER_DEFAULT;
			if (output == null || !output.CanWrite) return 0;
			output.SetLength(0);

			if (!thisValue.CanRead)
			{
				output.Flush();
				return 0;
			}

			if (thisValue.CanSeek) thisValue.Position = 0;

			int len;
			byte[] buffer = new byte[bufferSize];
			long written = 0;

			while ((len = thisValue.Read(buffer)) > 0)
			{
				output.Write(buffer);
				written += len;
			}

			output.Flush();
			if (output.CanSeek) output.Position = 0;
			return written;
		}

		public static int ReadExactly([NotNull] this Stream thisValue, [NotNull] byte[] buffer, int startIndex = 0, int length = -1)
		{
			if (!startIndex.InRangeRx(0, buffer.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (length == -1) length = buffer.Length - startIndex;
			if (length < 1 || startIndex + length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(length));

			int read = thisValue.Read(buffer, startIndex, length);
			if (read != length) throw new EndOfStreamException($"End of stream reached with {length - read} byte(s) left to read.");
			return read;
		}

		public static bool TryReadExactly([NotNull] this Stream thisValue, out int read, [NotNull] byte[] buffer, int startIndex = 0, int length = -1)
		{
			if (!startIndex.InRangeRx(0, buffer.Length)) throw new ArgumentOutOfRangeException(nameof(startIndex));
			if (length == -1) length = buffer.Length - startIndex;
			if (length < 1 || startIndex + length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(length));

			try
			{
				read = thisValue.Read(buffer, startIndex, length);
			}
			catch
			{
				read = 0;
			}

			return read == length;
		}

		public static Encoding DetectEncoding([NotNull] this Stream thisValue)
		{
			// seek to thisValue start
			if (thisValue.CanSeek && thisValue.Position > 0) thisValue.Position = 0;

			// buffer for preamble and up to 512b sample text for detection
			byte[] buf = new byte[Math.Min(thisValue.Length, Constants.GetBufferKB(4))];

			thisValue.Read(buf);
			Encoding encoding = EncodingHelper.GetEncoding(buf);
			// seek back to thisValue start
			if (thisValue.CanSeek) thisValue.Position = 0;

			return encoding;
		}

		public static bool SerializeBinary<T>([NotNull] this Stream thisValue, T value, StreamingContextStates contextStates = StreamingContextStates.Persistence, ISurrogateSelector selector = null)
		{
			if (value.IsNull()) return false;

			Type type = typeof(T);
			if (!type.IsSerializable) throw new ArgumentException($"Type {type.Name} is not serializable.", nameof(thisValue));

			IFormatter formatter = new BinaryFormatter { Context = new StreamingContext(contextStates) };
			if (selector != null) formatter.SurrogateSelector = selector;

			try
			{
				formatter.Serialize(thisValue, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static object DeserializeBinary([NotNull] this Stream thisValue, StreamingContextStates contextStates = StreamingContextStates.Persistence, ISurrogateSelector selector = null)
		{
			return DeserializeBinary(thisValue, (object)null, contextStates, selector);
		}

		public static T DeserializeBinary<T>([NotNull] this Stream thisValue, T defaultValue, StreamingContextStates contextStates = StreamingContextStates.Persistence, ISurrogateSelector selector = null)
		{
			IFormatter formatter = new BinaryFormatter { Context = new StreamingContext(contextStates) };
			if (selector != null) formatter.SurrogateSelector = selector;
			T value;

			try
			{
				value = (T)formatter.Deserialize(thisValue);
			}
			catch
			{
				value = defaultValue;
			}

			return value;
		}

		[NotNull]
		public static FileStream SaveToTempStream([NotNull] this Stream thisValue) { return SaveToTempStream(thisValue, null, null, 0); }

		[NotNull]
		public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath) { return SaveToTempStream(thisValue, basePath, null, 0); }

		[NotNull]
		public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath, string prefix) { return SaveToTempStream(thisValue, basePath, prefix, 0); }

		[NotNull]
		public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath, string prefix, uint unique)
		{
			FileStream stream = FileHelper.GetTempStream(basePath, prefix, unique);
			CopyTo(thisValue, stream);
			return stream;
		}

		public static string SaveToTempFile([NotNull] this Stream thisValue) { return SaveToTempFile(thisValue, null, null, 0); }

		public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath) { return SaveToTempFile(thisValue, basePath, null, 0); }

		public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath, string prefix) { return SaveToTempFile(thisValue, basePath, prefix, 0); }

		public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath, string prefix, uint unique)
		{
			string path;

			using (FileStream output = SaveToTempStream(thisValue, basePath, prefix, unique))
			{
				path = output.Name;
			}

			return path;
		}

		public static FileStream SaveToStream([NotNull] this Stream thisValue, [NotNull] string fileName, bool overwrite = false)
		{
			if (PathHelper.IsPathQualified(fileName))
			{
				string path = Path.GetDirectoryName(fileName);

				if (!string.IsNullOrEmpty(path))
				{
					if (!DirectoryHelper.Ensure(path)) return null;
				}
			}

			if (File.Exists(fileName))
			{
				if (!overwrite) return null;
				File.SetAttributes(fileName, File.GetAttributes(fileName) & ~(FileAttributes.Hidden | FileAttributes.ReadOnly));
			}

			FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
			CopyTo(thisValue, stream);
			return stream;
		}

		public static bool SaveToFile([NotNull] this Stream thisValue, [NotNull] string fileName, bool overwrite = false)
		{
			using (FileStream output = SaveToStream(thisValue, fileName, overwrite))
			{
				return output != null;
			}
		}

		public static MemoryStream SaveToMemory([NotNull] this Stream thisValue)
		{
			MemoryStream memoryStream = new MemoryStream();
			CopyTo(thisValue, memoryStream);
			if (memoryStream.Length == 0) ObjectHelper.Dispose(ref memoryStream);
			return memoryStream;
		}

		public static long CountLines([NotNull] this Stream thisValue, Encoding encoding = null)
		{
			if (thisValue.Length == 0) return 0;
			if (!thisValue.CanRead) throw new IOException("Stream cannot be read.");
			if (!thisValue.CanSeek) throw new IOException("Stream cannot seek.");

			thisValue.Position = 0;

			long len = 0L;
			int read;
			byte[] buffer = new byte[Constants.BUFFER_MB];
			Encoding enc = encoding ?? Encoding.UTF8;
			char prev = Constants.CHAR_NULL;
			char[] chars = new char[enc.GetMaxCharCount(buffer.Length)];
			bool pendingTermination = false;

			while ((read = thisValue.Read(buffer)) > 0)
			{
				read = enc.GetChars(buffer, 0, read, chars, 0);
				if (read < 1) continue;

				for (int i = 0; i < read; i++)
				{
					char cc = chars[i];
					if (cc == Constants.CHAR_NULL) continue;

					if (cc == Constants.CR || cc == Constants.LF)
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
			thisValue.Position = 0;
			return len;
		}

		public static long CountChar([NotNull] this Stream thisValue, char c, Encoding encoding = null)
		{
			if (thisValue.Length == 0) return 0;
			if (!thisValue.CanRead) throw new IOException("Stream cannot be read.");
			if (!thisValue.CanSeek) throw new IOException("Stream cannot seek.");

			thisValue.Position = 0;

			long len = 0L;
			int read;
			byte[] buffer = new byte[Constants.BUFFER_MB];
			Encoding enc = encoding ?? Encoding.UTF8;
			char[] chars = new char[enc.GetMaxCharCount(buffer.Length)];

			while ((read = thisValue.Read(buffer)) > 0)
			{
				read = enc.GetChars(buffer, 0, read, chars, 0);
				if (read < 1) continue;

				for (int i = 0; i < read; i++)
				{
					if (c == chars[i]) len++;
				}
			}

			thisValue.Position = 0;
			return len;
		}

		public static string GetFileName([NotNull] this Stream thisValue) { return (thisValue as FileStream)?.Name; }
	}
}