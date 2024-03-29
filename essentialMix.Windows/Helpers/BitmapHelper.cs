﻿using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using essentialMix.Drawing;
using essentialMix.Exceptions.Drawing;
using essentialMix.Extensions;
using essentialMix.Windows.Drawing;
using JetBrains.Annotations;
using EMBitmapHelper = essentialMix.Drawing.Helpers.BitmapHelper;

namespace essentialMix.Windows.Helpers;

public static class BitmapHelper
{
	private static readonly IReadOnlyDictionary<PixelFormat, PixelFormat> __formatTranslations = new Dictionary<PixelFormat, PixelFormat>
	{
		{ PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed },
		{ PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb },
		{ PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb },
		{ PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb },
		{ PixelFormat.Format16bppGrayScale, PixelFormat.Format16bppGrayScale },
		{ PixelFormat.Format48bppRgb, PixelFormat.Format48bppRgb },
		{ PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb }
	}.AsReadOnly();

	public static Bitmap Crop([NotNull] Bitmap value, Rectangle rectangle, DimensionRestriction restriction = DimensionRestriction.None)
	{
		// Check source format
		if (!__formatTranslations.TryGetValue(value.PixelFormat, out PixelFormat formatTranslation)) throw new UnsupportedImageFormatException();
		if (value.Width == 0 || value.Height == 0 || rectangle.IsEmpty) return null;

		int width = value.Width, height = value.Height;

		if (restriction.FastHasFlag(DimensionRestriction.KeepWidth))
		{
			rectangle.X = 0;
			rectangle.Width = width;
		}

		if (restriction.FastHasFlag(DimensionRestriction.KeepHeight))
		{
			rectangle.Y = 0;
			rectangle.Height = height;
		}

		if (rectangle.IsEmpty || rectangle.Width <= 0 || rectangle.Height <= 0) return null;
		if (rectangle.X == 0 && rectangle.Y == 0 && rectangle.Width == width && rectangle.Height == height) return Clone(value);
		if (rectangle.X + rectangle.Width > width) rectangle.Width = width - rectangle.X;
		if (rectangle.Y + rectangle.Height > height) rectangle.Height = height - rectangle.Y;

		BitmapData imageData = value.LockBits(new Rectangle(0, 0, value.Width, value.Height), ImageLockMode.ReadOnly, value.PixelFormat);
		Bitmap bitmap;

		try
		{
			Size newImageSize = rectangle.Size;
			bitmap = formatTranslation == PixelFormat.Format8bppIndexed
						? EMBitmapHelper.CreateGreyscaleImage(newImageSize.Width, newImageSize.Height)
						: new Bitmap(newImageSize.Width, newImageSize.Height, formatTranslation);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, newImageSize.Width, newImageSize.Height), ImageLockMode.ReadWrite, formatTranslation);

			try
			{
				// Process filter
				Crop(new UnmanagedImage(imageData), new UnmanagedImage(bitmapData), rectangle);
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}

			if (value.HorizontalResolution > 0.0)
			{
				if (value.VerticalResolution > 0.0)
					bitmap.SetResolution(value.HorizontalResolution, value.VerticalResolution);
			}
		}
		finally
		{
			value.UnlockBits(imageData);
		}

		return bitmap;
	}

	public static Bitmap CropBackground([NotNull] Bitmap value, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
	{
		Color? backColor = EMBitmapHelper.GetMatchedBackColor(value);
		return !backColor.HasValue ? value : CropBackground(value, backColor.Value, restriction, unitWidthLimit, unitHeightLimit);
	}

	public static Bitmap CropBackground([NotNull] Bitmap value, Color backColor, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
	{
		(Point TopLeft, Point BottomRight) bounds = EMBitmapHelper.FindImageBounds(value, backColor, restriction, unitWidthLimit, unitHeightLimit);
		if (bounds.TopLeft.IsEmpty && bounds.BottomRight.IsEmpty) return null;
		if (bounds.TopLeft.IsEmpty && bounds.BottomRight.X == value.Width && bounds.BottomRight.Y == value.Height) return Clone(value);
		int diffX = bounds.BottomRight.X - bounds.TopLeft.X + 1;
		int diffY = bounds.BottomRight.Y - bounds.TopLeft.Y + 1;
		Rectangle destRect = new Rectangle(0, 0, diffX, diffY);
		return Crop(value, destRect);
	}

	[NotNull]
	public static Bitmap Clone([NotNull] Bitmap value)
	{
		// lock source bitmap data
		BitmapData sourceData = value.LockBits(new Rectangle(0, 0, value.Width, value.Height), ImageLockMode.ReadOnly, value.PixelFormat);

		// create new image
		Bitmap destination;

		try
		{
			destination = Clone(sourceData);
		}
		finally
		{
			value.UnlockBits(sourceData);
		}

		switch (value.PixelFormat)
		{
			case PixelFormat.Format1bppIndexed:
			case PixelFormat.Format4bppIndexed:
			case PixelFormat.Format8bppIndexed:
			case PixelFormat.Indexed:
				ColorPalette srcPalette = value.Palette;
				ColorPalette dstPalette = destination.Palette;
				int n = srcPalette.Entries.Length;

				// copy palette
				for (int i = 0; i < n; i++)
				{
					dstPalette.Entries[i] = srcPalette.Entries[i];
				}

				destination.Palette = dstPalette;
				break;
		}

		return destination;
	}

	[NotNull]
	public static Bitmap Clone([NotNull] Bitmap value, PixelFormat format)
	{
		// copy image if pixel format is the same
		if (value.PixelFormat == format) return Clone(value);

		int width = value.Width;
		int height = value.Height;

		// create new image with desired pixel format
		Bitmap bitmap = new Bitmap(width, height, format);

		// draw source image on the new one using Graphics
		using (Graphics g = Graphics.FromImage(bitmap))
		{
			g.DrawImage(value, 0, 0, width, height);
		}

		return bitmap;
	}

	[NotNull]
	public static Bitmap Clone([NotNull] BitmapData value)
	{
		// get source image size
		int width = value.Width;
		int height = value.Height;
		Bitmap destination = new Bitmap(width, height, value.PixelFormat);

		// lock destination bitmap data
		BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, destination.PixelFormat);

		try
		{
			MemoryHelper.CopyMemory(value.Scan0, destinationData.Scan0, height * value.Stride);
		}
		finally
		{
			destination.UnlockBits(destinationData);
		}

		return destination;
	}

	private static unsafe void Crop([NotNull] UnmanagedImage sourceData, [NotNull] UnmanagedImage destinationData, Rectangle rectangle)
	{
		rectangle.Intersect(new Rectangle(0, 0, sourceData.Width, sourceData.Height));

		int xMin = rectangle.Left;
		int yMin = rectangle.Top;
		int yMax = rectangle.Bottom - 1;
		int copyWidth = rectangle.Width;

		int srcStride = sourceData.Stride;
		int dstStride = destinationData.Stride;
		int pixelSize = Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
		int copySize = copyWidth * pixelSize;

		// do the job
		byte* src = (byte*)sourceData.ImageData.ToPointer() + yMin * srcStride + xMin * pixelSize;
		byte* dst = (byte*)destinationData.ImageData.ToPointer();

		if (rectangle.Top < 0) dst -= dstStride * rectangle.Top;
		if (rectangle.Left < 0) dst -= pixelSize * rectangle.Left;

		// for each line
		for (int y = yMin; y <= yMax; y++)
		{
			MemoryHelper.CopyMemory(src, dst, copySize);
			src += srcStride;
			dst += dstStride;
		}
	}
}