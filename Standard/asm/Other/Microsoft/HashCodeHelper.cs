using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Other.Microsoft
{
	public class HashCodeHelper
	{
		private static readonly Lazy<BinaryFormatter> __formatter = new Lazy<BinaryFormatter>(() => new BinaryFormatter(), LazyThreadSafetyMode.ExecutionAndPublication);
		private static readonly Lazy<HashAlgorithm> __hasher = new Lazy<HashAlgorithm>(() => new SHA384CryptoServiceProvider(), LazyThreadSafetyMode.ExecutionAndPublication);
		private static readonly Lazy<ConditionalWeakTable<object, SerializationInfo>> __serializationInfoTable = new Lazy<ConditionalWeakTable<object, SerializationInfo>>(() => new ConditionalWeakTable<object, SerializationInfo>(), LazyThreadSafetyMode.ExecutionAndPublication);

		internal HashCodeHelper()
		{
			// Start with a seed (obtained from String.GetHashCode implementation)
			CombinedHash = 5381;
		}

		internal HashCodeHelper([NotNull] HashCodeHelper hcc) { CombinedHash = hcc.CombinedHash; }

		internal HashCodeHelper(long initialCombinedHash) { CombinedHash = initialCombinedHash; }

		internal int CombinedHash32 => CombinedHash.GetHashCode();

		[NotNull]
		internal string CombinedHashString => CombinedHash.ToString("x", CultureInfoHelper.Default);

		internal long CombinedHash { get; private set; }

		internal void AddArray(string[] a)
		{
			if (a == null) return;

			int n = a.Length;

			for (int i = 0; i < n; i++) AddObject(a[i]);
		}

		internal void AddCaseInsensitiveString(string s)
		{
			if (s != null) AddInt(StringComparer.InvariantCultureIgnoreCase.GetHashCode(s));
		}

		internal void AddDateTime(DateTime dt) { AddInt(dt.GetHashCode()); }

		internal void AddDirectory([NotNull] string directoryName)
		{
			DirectoryInfo directory = new DirectoryInfo(directoryName);
			if (!directory.Exists) return;

			AddObject(directoryName);

			foreach (DirectoryInfo directoryInfo in directory.GetDirectories()) AddDirectory(directoryInfo.FullName);

			foreach (FileInfo fileInfo in directory.GetFiles()) AddExistingFile(fileInfo.FullName);

			AddDateTime(directory.CreationTimeUtc);
			AddDateTime(directory.LastWriteTimeUtc);
		}

		internal void AddFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				// Review: Should we change the dependency model to take directory into account?
				if (Directory.Exists(fileName))
				{
					// Add as a directory dependency if it's a directory.
					AddDirectory(fileName);
					return;
				}

				return;
			}

			AddExistingFile(fileName);
		}

		internal void AddInt(int n) { CombinedHash = ((CombinedHash << 5) + CombinedHash) ^ n; }

		internal void AddObject(int n) { AddInt(n); }

		internal void AddObject(byte b) { AddInt(b.GetHashCode()); }

		internal void AddObject(long l) { AddInt(l.GetHashCode()); }

		internal void AddObject(bool b) { AddInt(b.GetHashCode()); }

		internal void AddObject(string s)
		{
			if (s == null) return;
			AddInt(s.GetHashCode());
		}

		internal void AddObject(object o)
		{
			if (o == null) return;
			AddInt(o.GetHashCode());
		}

		// Same as AddDirectory, but only look at files that don't have a culture
		internal void AddResourcesDirectory([NotNull] string directoryName)
		{
			DirectoryInfo directory = new DirectoryInfo(directoryName);
			if (!directory.Exists) return;

			AddObject(directoryName);

			// Go through all the files in the directory
			foreach (DirectoryInfo directoryInfo in directory.GetDirectories()) AddResourcesDirectory(directoryInfo.FullName);

			foreach (FileInfo fileInfo in directory.GetFiles())
			{
				// Ignore the file if it has a culture, since only neutral files
				// need to re-trigger compilation (VSWhidbey 359029)
				string fullPath = fileInfo.FullName;

				if (CultureInfoHelper.GetNameFromFileName(fullPath) == null) AddExistingFile(fullPath);
			}

			AddDateTime(directory.CreationTimeUtc);
		}

		private void AddExistingFile([NotNull] string fileName)
		{
			if (File.Exists(fileName)) throw new FileNotFoundException();

			AddInt(fileName.GetHashCode());
			FileInfo file = new FileInfo(fileName);
			AddDateTime(file.CreationTimeUtc);
			AddDateTime(file.LastWriteTimeUtc);
			AddFileSize(file.Length);
		}

		private void AddFileSize(long fileSize) { AddInt(fileSize.GetHashCode()); }

		public static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable => __serializationInfoTable.Value;

		public static int CombineHashCodes(int h1, int h2) { return ((h1 << 5) + h1) ^ h2; }

		public static int CombineHashCodes(int h1, int h2, int h3) { return CombineHashCodes(CombineHashCodes(h1, h2), h3); }

		public static int CombineHashCodes(int h1, int h2, int h3, int h4) { return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4)); }

		public static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) { return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5); }

		public static ulong GetHashCode<T>(T value)
		{
			byte[] bytes = ToBytes(value, typeof(T).AsTypeCode(), stream => __hasher.Value.ComputeHash(stream));
			if (bytes.Length < Constants.ULONG_SIZE) Array.Resize(ref bytes, Constants.ULONG_SIZE);
			return BitConverter.ToUInt64(bytes, 0);
		}

		public static byte[] ToBytes<T>(T value) { return ToBytes(value, typeof(T)); }
		public static byte[] ToBytes(object value, Type type) { return ToBytes(value, (type ?? value?.GetType() ?? throw new InvalidOperationException()).AsTypeCode()); }
		private static byte[] ToBytes(object value, TypeCode type, Func<Stream, byte[]> onStreaming = null)
		{
			switch (type)
			{
				case TypeCode.DBNull:
				case TypeCode.Empty:
				{
					return null;
				}
				case TypeCode.Boolean:
					return BitConverter.GetBytes(Convert.ToBoolean(value));
				case TypeCode.Char:
					return BitConverter.GetBytes(Convert.ToChar(value));
				case TypeCode.Byte:
					return BitConverter.GetBytes(Convert.ToByte(value));
				case TypeCode.SByte:
					return BitConverter.GetBytes(Convert.ToSByte(value));
				case TypeCode.Int16:
					return BitConverter.GetBytes(Convert.ToInt16(value));
				case TypeCode.UInt16:
					return BitConverter.GetBytes(Convert.ToUInt16(value));
				case TypeCode.Int32:
					return BitConverter.GetBytes(Convert.ToInt32(value));
				case TypeCode.UInt32:
					return BitConverter.GetBytes(Convert.ToUInt32(value));
				case TypeCode.Int64:
					return BitConverter.GetBytes(Convert.ToInt64(value));
				case TypeCode.UInt64:
					return BitConverter.GetBytes(Convert.ToUInt64(value));
				case TypeCode.Single:
					return BitConverter.GetBytes(Convert.ToSingle(value));
				case TypeCode.Double:
					return BitConverter.GetBytes(Convert.ToDouble(value));
				case TypeCode.Decimal:
					return Convert.ToDecimal(value).GetBytes();
				default:
					MemoryStream stream = null;

					try
					{
						stream = new MemoryStream();
						__formatter.Value.Serialize(stream, value);
						stream.Flush();
						stream.Position = 0;
						return onStreaming?.Invoke(stream) ?? stream.ToArray();
					}
					finally
					{
						ObjectHelper.Dispose(ref stream);
					}
			}
		}
	}
}