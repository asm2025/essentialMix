using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class ImageExtension
	{
		[NotNull]
		public static byte[] ToByteArray([NotNull] this Image thisValue)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				thisValue.Save(stream, thisValue.RawFormat.HasEncoder() ? thisValue.RawFormat : ImageFormat.Bmp);
				return stream.ToArray();
			}
		}
	}
}