using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using essentialMix.Exceptions.Drawing;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Drawing.Helpers;

public static class BitmapHelper
{
	public static (Point TopLeft, Point BottomRight) FindImageBounds([NotNull] Bitmap value, Color backColor, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
	{
		if (value.Width < 2 || value.Height < 2) return (Point.Empty, Point.Empty);

		int width = value.Width, height = value.Height;
		Point topLeft = new Point();
		Point bottomRight = new Point(width, height);
		unitWidthLimit = unitWidthLimit.Within(0, width);
		unitHeightLimit = unitHeightLimit.Within(0, height);
		if (unitWidthLimit == width) restriction |= DimensionRestriction.KeepWidth;
		if (unitHeightLimit == height) restriction |= DimensionRestriction.KeepHeight;

		bool kw = restriction.FastHasFlag(DimensionRestriction.KeepWidth);
		bool kh = restriction.FastHasFlag(DimensionRestriction.KeepHeight);
		if (kw && kh) return (topLeft, bottomRight);

		bool kuw = unitWidthLimit > 0;
		bool kuh = unitHeightLimit > 0;
		bool found = false;

		if (!kuw || !kuh)
		{
			for (int y = 0; y < height && !found; y++)
			{
				for (int x = 0; x < width && !found; x++)
				{
					Color c = value.GetPixel(x + 1, y + 1);
					if (c.IsSame(backColor)) continue;
					if (!kw && !kuw) topLeft.X = x;
					if (!kh && !kuh) topLeft.Y = y;
					found = true;
				}
			}

			found = false;
		}

		for (int y = height; y > 0 && !found; y--)
		{
			for (int x = width; x > 0 && !found; x--)
			{
				Color c = value.GetPixel(x - 1, y - 1);
				if (c.IsSame(backColor)) continue;
				if (!kw) bottomRight.X = x;
				if (!kh) bottomRight.Y = y;
				found = true;
			}
		}

		if (kuw && !kw) bottomRight.X = bottomRight.X.Multiplier(unitWidthLimit);
		if (kuh && !kh) bottomRight.Y = bottomRight.Y.Multiplier(unitHeightLimit);
		return (topLeft, bottomRight);
	}

	public static Color? GetMatchedBackColor(Bitmap value)
	{
		if (value == null || value.Width < 2 || value.Height < 2) return null;

		// Getting The Background Color by checking Corners of Original Image
		Point[] corners =
		[
			new Point(1, 1),
			new Point(1, value.Height - 1),
			new Point(value.Width - 1, 1),
			new Point(value.Width - 1, value.Height - 1)
		]; // four corners (Top, Left), (Top, Right), (Bottom, Left), (Bottom, Right)

		foreach (Point point in corners)
		{
			Color backColor = value.GetPixel(point.X, point.Y);
			int cornerMatched = corners.Select(t => value.GetPixel(t.X, t.Y)).Count(cornerColor => cornerColor.IsSame(backColor));
			if (cornerMatched > 2) return backColor;
		}

		return null;
	}

	[NotNull]
	public static Bitmap CreateGreyscaleImage(Size size) { return CreateGreyscaleImage(size.Width, size.Height); }
	[NotNull]
	public static Bitmap CreateGreyscaleImage(int width, int height)
	{
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
		ImageHelper.SetGreyscalePalette(bitmap);
		return bitmap;
	}

	[NotNull]
	public static Bitmap Convert16bppTo8bpp([NotNull] Bitmap value)
	{
		Bitmap newImage;
		int layers;
		int width = value.Width;
		int height = value.Height;

		// create new image depending on source image format
		switch (value.PixelFormat)
		{
			case PixelFormat.Format16bppGrayScale:
				// create new greyscale image
				newImage = CreateGreyscaleImage(width, height);
				layers = 1;
				break;
			case PixelFormat.Format48bppRgb:
				// create new color 24 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
				layers = 3;
				break;
			case PixelFormat.Format64bppArgb:
				// create new color 32 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				layers = 4;
				break;
			case PixelFormat.Format64bppPArgb:
				// create new color 32 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
				layers = 4;
				break;
			default:
				throw new UnsupportedImageFormatException();
		}

		// lock both images
		BitmapData sourceData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, value.PixelFormat);
		BitmapData newData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, newImage.PixelFormat);

		try
		{
			unsafe
			{
				byte* sourceBasePtr = (byte*)sourceData.Scan0.ToPointer();
				byte* newBasePtr = (byte*)newData.Scan0.ToPointer();
				int sourceStride = sourceData.Stride;
				int newStride = newData.Stride;

				for (int y = 0; y < height; y++)
				{
					ushort* sourcePtr = (ushort*)(sourceBasePtr + y * sourceStride);
					byte* newPtr = newBasePtr + y * newStride;

					for (int x = 0, lineSize = width * layers; x < lineSize; x++, sourcePtr++, newPtr++)
					{
						*newPtr = (byte)(*sourcePtr >> 8);
					}
				}
			}
		}
		finally
		{
			value.UnlockBits(sourceData);
			newImage.UnlockBits(newData);
		}

		return newImage;
	}

	[NotNull]
	public static Bitmap Convert8bppTo16bpp([NotNull] Bitmap value)
	{
		Bitmap newImage;
		int layers;
		int width = value.Width;
		int height = value.Height;

		// create new image depending on source image format
		switch (value.PixelFormat)
		{
			case PixelFormat.Format8bppIndexed:
				// create new greyscale image
				newImage = new Bitmap(width, height, PixelFormat.Format16bppGrayScale);
				layers = 1;
				break;
			case PixelFormat.Format24bppRgb:
				// create new color 48 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format48bppRgb);
				layers = 3;
				break;
			case PixelFormat.Format32bppArgb:
				// create new color 64 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format64bppArgb);
				layers = 4;
				break;
			case PixelFormat.Format32bppPArgb:
				// create new color 64 bpp image
				newImage = new Bitmap(width, height, PixelFormat.Format64bppPArgb);
				layers = 4;
				break;
			default:
				throw new UnsupportedImageFormatException("Invalid pixel format of the source image.");
		}

		// lock both images
		BitmapData sourceData = value.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, value.PixelFormat);
		BitmapData newData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, newImage.PixelFormat);

		try
		{
			unsafe
			{
				// base pointers
				byte* sourceBasePtr = (byte*)sourceData.Scan0.ToPointer();
				byte* newBasePtr = (byte*)newData.Scan0.ToPointer();
				// image strides
				int sourceStride = sourceData.Stride;
				int newStride = newData.Stride;

				for (int y = 0; y < height; y++)
				{
					byte* sourcePtr = sourceBasePtr + y * sourceStride;
					ushort* newPtr = (ushort*)(newBasePtr + y * newStride);

					for (int x = 0, lineSize = width * layers; x < lineSize; x++, sourcePtr++, newPtr++)
					{
						*newPtr = (ushort)(*sourcePtr << 8);
					}
				}
			}
		}
		finally
		{
			value.UnlockBits(sourceData);
			newImage.UnlockBits(newData);
		}

		return newImage;
	}
}