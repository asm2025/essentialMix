using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Drawing.Extensions
{
	public static class ImageFormatExtension
	{
		public static ImageCodecInfo GetEncoderInfo([NotNull] this ImageFormat thisValue)
		{
			return ImageCodecInfo.GetImageEncoders()
				.FirstOrDefault(encoder => encoder.FormatID == thisValue.Guid);
		}

		public static bool HasEncoder([NotNull] this ImageFormat thisValue) { return GetEncoderInfo(thisValue) != null; }
	}
}