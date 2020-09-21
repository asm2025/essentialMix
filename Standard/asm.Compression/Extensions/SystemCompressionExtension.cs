using System;
using System.IO.Compression;
using CompressionLevel = asm.Compression.CompressionLevel;
using CompressionMode = asm.Compression.CompressionMode;
using SystemCompressionLevel = System.IO.Compression.CompressionLevel;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class SystemCompressionExtension
	{
		public static SystemCompressionLevel ToSystemCompressionLevel(this CompressionLevel thisValue)
		{
			switch (thisValue)
			{
				case CompressionLevel.None:
					return SystemCompressionLevel.NoCompression;
				case CompressionLevel.Fast:
				case CompressionLevel.Low:
				case CompressionLevel.Normal:
					return SystemCompressionLevel.Fastest;
				case CompressionLevel.High:
				case CompressionLevel.Ultra:
					return SystemCompressionLevel.Optimal;
				default:
					throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null);
			}
		}
		
		public static CompressionLevel ToCompressionLevel(this SystemCompressionLevel thisValue)
		{
			switch (thisValue)
			{
				case SystemCompressionLevel.NoCompression:
					return CompressionLevel.None;
				case SystemCompressionLevel.Fastest:
					return CompressionLevel.Fast;
				case SystemCompressionLevel.Optimal:
					return CompressionLevel.Ultra;
				default:
					throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null);
			}
		}

		public static ZipArchiveMode ToSystemCompressionMode(this CompressionMode thisValue)
		{
			switch (thisValue)
			{
				case CompressionMode.Create:
					return ZipArchiveMode.Create;
				case CompressionMode.Append:
					return ZipArchiveMode.Update;
				default:
					throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null);
			}
		}
		
		public static CompressionMode ToCompressionMode(this ZipArchiveMode thisValue)
		{
			switch (thisValue)
			{
				case ZipArchiveMode.Read:
				case ZipArchiveMode.Create:
					return CompressionMode.Create;
				case ZipArchiveMode.Update:
					return CompressionMode.Append;
				default:
					throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null);
			}
		}
	}
}