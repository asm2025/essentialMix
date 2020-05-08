using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using asm.Drawing.Extensions;
using asm.Exceptions.Drawing;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Drawing.Helpers
{
	public static class BitmapHelper
	{
		private static readonly IReadOnlyDictionary<PixelFormat, PixelFormat> __formatTranslations = new ReadOnlyDictionary<PixelFormat, PixelFormat>(new Dictionary<PixelFormat, PixelFormat>
		{
			{ PixelFormat.Format8bppIndexed, PixelFormat.Format8bppIndexed },
			{ PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb },
			{ PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb },
			{ PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb },
			{ PixelFormat.Format16bppGrayScale, PixelFormat.Format16bppGrayScale },
			{ PixelFormat.Format48bppRgb, PixelFormat.Format48bppRgb },
			{ PixelFormat.Format64bppArgb, PixelFormat.Format64bppArgb }
		});

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

			bool kw = restriction.HasFlag(DimensionRestriction.KeepWidth);
			bool kh = restriction.HasFlag(DimensionRestriction.KeepHeight);
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
			Point[] corners = {
				new Point(1, 1),
				new Point(1, value.Height - 1),
				new Point(value.Width - 1, 1),
				new Point(value.Width - 1, value.Height - 1)
			}; // four corners (Top, Left), (Top, Right), (Bottom, Left), (Bottom, Right)

			foreach (Point point in corners)
			{
				Color backColor = value.GetPixel(point.X, point.Y);
				int cornerMatched = corners.Select(t => value.GetPixel(t.X, t.Y)).Count(cornerColor => cornerColor.IsSame(backColor));
				if (cornerMatched > 2) return backColor;
			}

			return null;
		}

		public static Bitmap Crop([NotNull] Bitmap value, Rectangle rectangle, DimensionRestriction restriction = DimensionRestriction.None)
		{
			// Check source format
			if (!__formatTranslations.TryGetValue(value.PixelFormat, out PixelFormat formatTranslation)) throw new UnsupportedImageFormatException();
			if (value.Width == 0 || value.Height == 0 || rectangle.IsEmpty) return null;

			int width = value.Width, height = value.Height;

			if (restriction.HasFlag(DimensionRestriction.KeepWidth))
			{
				rectangle.X = 0;
				rectangle.Width = width;
			}

			if (restriction.HasFlag(DimensionRestriction.KeepHeight))
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
									? CreateGreyscaleImage(newImageSize.Width, newImageSize.Height)
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
			Color? backColor = GetMatchedBackColor(value);
			return !backColor.HasValue ? value : CropBackground(value, backColor.Value, restriction, unitWidthLimit, unitHeightLimit);
		}

		public static Bitmap CropBackground([NotNull] Bitmap value, Color backColor, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
		{
			(Point TopLeft, Point BottomRight) bounds = FindImageBounds(value, backColor, restriction, unitWidthLimit, unitHeightLimit);
			if (bounds.TopLeft.IsEmpty && bounds.BottomRight.IsEmpty) return null;
			if (bounds.TopLeft.IsEmpty && bounds.BottomRight.X == value.Width && bounds.BottomRight.Y == value.Height) return Clone(value);
			int diffX = bounds.BottomRight.X - bounds.TopLeft.X + 1;
			int diffY = bounds.BottomRight.Y - bounds.TopLeft.Y + 1;
			Rectangle destRect = new Rectangle(0, 0, diffX, diffY);
			return Crop(value, destRect);
		}

		[NotNull] public static Bitmap CreateGreyscaleImage(Size size) { return CreateGreyscaleImage(size.Width, size.Height); }
		[NotNull]
		public static Bitmap CreateGreyscaleImage(int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
			ImageHelper.SetGreyscalePalette(bitmap);
			return bitmap;
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

		private static unsafe void Crop([NotNull] UnmanagedImage sourceData, [NotNull] UnmanagedImage destinationData, Rectangle rectangle)
		{
			rectangle.Intersect(new Rectangle(0, 0, sourceData.Width, sourceData.Height));

			int x_min = rectangle.Left;
			int y_min = rectangle.Top;
			int y_max = rectangle.Bottom - 1;
			int copyWidth = rectangle.Width;

			int srcStride = sourceData.Stride;
			int dstStride = destinationData.Stride;
			int pixelSize = Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
			int copySize = copyWidth * pixelSize;

			// do the job
			byte* src = (byte*)sourceData.ImageData.ToPointer() + y_min * srcStride + x_min * pixelSize;
			byte* dst = (byte*)destinationData.ImageData.ToPointer();

			if (rectangle.Top < 0) dst -= dstStride * rectangle.Top;
			if (rectangle.Left < 0) dst -= pixelSize * rectangle.Left;

			// for each line
			for (int y = y_min; y <= y_max; y++)
			{
				MemoryHelper.CopyMemory(src, dst, copySize);
				src += srcStride;
				dst += dstStride;
			}
		}
	}
}