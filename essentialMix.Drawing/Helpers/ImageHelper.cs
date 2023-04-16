using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using essentialMix.Exceptions.Drawing;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Image = System.Drawing.Image;

namespace essentialMix.Drawing.Helpers;

public static class ImageHelper
{
	public static Image GetOrSave([NotNull] Stream stream, [NotNull] string fileName, ImageFormat format = null, bool useEmbeddedColorManagement = false, bool validateImageData = true)
	{
		return File.Exists(fileName)
					? Image.FromFile(fileName, useEmbeddedColorManagement)
					: Save(stream, ref fileName, false, format, useEmbeddedColorManagement, validateImageData);
	}

	[NotNull]
	public static Image Save([NotNull] Stream stream, [NotNull] ref string fileName, bool renameOnExists = false, ImageFormat format = null, bool useEmbeddedColorManagement = false, bool validateImageData = true)
	{
		Image image = null;
		if (stream.CanSeek) stream.Position = 0;

		try
		{
			image = Image.FromStream(stream, useEmbeddedColorManagement, validateImageData);
			fileName = Save(image, fileName, renameOnExists, format);
		}
		catch
		{
			ObjectHelper.Dispose(ref image);
			throw;
		}

		return image;
	}

	[NotNull]
	public static string Save([NotNull] Image image, [NotNull] string fileName, bool renameOnExists = false, ImageFormat format = null)
	{
		fileName = PathHelper.Trim(fileName);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		string path = Path.GetDirectoryName(fileName);

		if (path == null)
			path = Directory.GetCurrentDirectory();
		else
			fileName = Path.GetFileName(fileName);

		if (!DirectoryHelper.Ensure(path)) throw new DirectoryNotFoundException();

		if (renameOnExists)
		{
			string ext = Path.GetExtension(fileName);
			if (!ext.StartsWith('.')) ext = "." + ext;
			fileName = Path.GetFileNameWithoutExtension(fileName);

			string combinedPath;
			int i = 0;

			do
			{
				combinedPath = Path.Combine(path, fileName +
												(i++ > 0
													? $" ({i})"
													: string.Empty) +
												ext);
			}
			while (PathHelper.Exists(combinedPath));

			fileName = combinedPath;
		}
		else
		{
			fileName = Path.Combine(path, fileName);
		}

		image.Save(fileName, format ?? image.RawFormat);
		return fileName;
	}

	[NotNull]
	public static string SaveAsJpeg([NotNull] Image image, [NotNull] string fileName, bool renameOnExists = false, byte quality = 85)
	{
		fileName = PathHelper.Trim(fileName);
		if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

		string path = Path.GetDirectoryName(fileName);

		if (path == null)
			path = Directory.GetCurrentDirectory();
		else
			fileName = Path.GetFileName(fileName);

		if (!DirectoryHelper.Ensure(path)) throw new DirectoryNotFoundException();

		if (renameOnExists)
		{
			fileName = Path.GetFileNameWithoutExtension(fileName);

			string combinedPath;
			int i = 0;

			do
			{
				combinedPath = Path.Combine(path, fileName +
												(i++ > 0
													? $" ({i})"
													: string.Empty) +
												".jpg");
			}
			while (PathHelper.Exists(combinedPath));

			fileName = combinedPath;
		}
		else
		{
			fileName = Path.Combine(path, Path.GetFileNameWithoutExtension(fileName) + ".jpg");
		}

		FileStream fileStream = null;

		try
		{
			fileStream = File.OpenWrite(fileName);
			ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
			EncoderParameters encoderParameter = new EncoderParameters(1);
			encoderParameter.Param[0] = new EncoderParameter(Encoder.Quality, quality);
			image.Save(fileStream, codecInfo, encoderParameter);
		}
		finally
		{
			ObjectHelper.Dispose(fileStream);
		}

		return fileName;
	}

	[NotNull]
	public static Image Resize([NotNull] Image image, int value, bool resizeToX = true)
	{
		(int x, int y) = Numeric.Math2.AspectRatio(image.Width, image.Height, value, resizeToX);
		return Resize(image, x, y);
	}

	[NotNull]
	public static Image Resize([NotNull] Image image, int width, int height)
	{
		if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
		if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

		Rectangle rect = new Rectangle(0, 0, width, height);
		Bitmap bitmap = null;
		Graphics graphics = null;
		ImageAttributes imageAttributes = null;

		try
		{
			bitmap = new Bitmap(width, height);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			graphics = Graphics.FromImage(bitmap);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			imageAttributes = new ImageAttributes();
			imageAttributes.SetWrapMode(WrapMode.TileFlipXY, Color.Transparent);

			graphics.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			return bitmap;
		}
		catch
		{
			ObjectHelper.Dispose(ref bitmap);
			throw;
		}
		finally
		{
			ObjectHelper.Dispose(ref graphics);
			ObjectHelper.Dispose(ref imageAttributes);
		}
	}

	[NotNull]
	public static Image ResizeWithRatio([NotNull] Image image, int maxWidth, int maxHeight)
	{
		if (maxWidth <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidth));
		if (maxHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maxHeight));

		Size size = GetThumbnailSize(image.Width, image.Height, maxWidth, maxHeight);
		if (size.IsEmpty || (image.Width == size.Width && image.Height == size.Height)) return image;

		int x = Convert.ToInt32((maxWidth - size.Width) / 2.0d);
		int y = Convert.ToInt32((maxHeight - size.Height) / 2.0d);
		Rectangle destRect = new Rectangle(x, y, size.Width, size.Height);
		Bitmap bitmap = null;
		Graphics graphics = null;
		ImageAttributes imageAttributes = null;

		try
		{
			bitmap = new Bitmap(maxWidth, maxHeight);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			imageAttributes = new ImageAttributes();
			imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

			graphics = Graphics.FromImage(bitmap);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.Clear(Color.White);
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

			return bitmap;
		}
		catch
		{
			ObjectHelper.Dispose(ref bitmap);
			throw;
		}
		finally
		{
			ObjectHelper.Dispose(ref graphics);
			ObjectHelper.Dispose(ref imageAttributes);
		}
	}

	[NotNull]
	public static Image Crop([NotNull] Image image, int width, int height)
	{
		if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
		if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
		if (image.Width <= width && image.Height <= height) return image;

		int x = Convert.ToInt32((image.Width - width) / 2.0d);
		int y = Convert.ToInt32((image.Height - height) / 2.0d);
		Rectangle rcSource = new Rectangle(x, y, width, height);
		Rectangle rcDestination = new Rectangle(0, 0, width, height);
		Bitmap bitmap = null;
		Graphics graphics = null;

		try
		{
			bitmap = new Bitmap(rcDestination.Width, rcDestination.Height);
			bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			graphics = Graphics.FromImage(bitmap);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			graphics.DrawImage(image, rcDestination, rcSource, GraphicsUnit.Pixel);

			return bitmap;
		}
		catch
		{
			ObjectHelper.Dispose(ref bitmap);
			throw;
		}
		finally
		{
			ObjectHelper.Dispose(ref graphics);
		}
	}

	public static ImageCodecInfo GetEncoderInfo([NotNull] string mimeType)
	{
		return ImageCodecInfo.GetImageEncoders()
							.FirstOrDefault(encoder => encoder.MimeType.IsSame(mimeType));
	}

	public static Size GetImageSizeFromFile([NotNull] string fileName)
	{
		if (!File.Exists(fileName)) return Size.Empty;

		using (Image img = Image.FromFile(fileName))
		{
			return img.Size;
		}
	}

	public static Size GetThumbnailSize(int width, int height, int maxWidth, int maxHeight)
	{
		Size pSize = new Size(maxWidth, maxHeight);

		int nOw = width;
		int nOh = height;

		if (nOw == 0 && nOh == 0) return pSize;
		if (nOw == 0 || nOh == 0) return pSize;

		int nNw = maxWidth < 10 ? 10 : maxWidth;
		int nNh = maxHeight < 10 ? 10 : maxHeight;

		if (nOw > nNw || nOh > nNh)
		{
			float fX = nOw / (float)nNw;
			float fY = nOh / (float)nNh;

			if (fY > fX)
				nNw = Convert.ToInt32(nOw / fY);
			else
				nNh = Convert.ToInt32(nOh / fX);

			pSize.Width = nNw;
			pSize.Height = nNh;
		}
		else
		{
			pSize.Width = nOw;
			pSize.Height = nOh;
		}

		return pSize;
	}

	public static Image FromByteArray([NotNull] byte[] bytes, bool useEmbeddedColorManagement = false, bool validateImageData = true)
	{
		if (bytes.Length == 0) return null;

		using (MemoryStream stream = new MemoryStream(bytes))
		{
			return Image.FromStream(stream, useEmbeddedColorManagement, validateImageData);
		}
	}

	public static bool IsGreyscale([NotNull] Image value)
	{
		if (value.PixelFormat != PixelFormat.Format8bppIndexed) return false;

		ColorPalette palette = value.Palette;

		for (int i = 0; i < 256; i++)
		{
			Color entry = palette.Entries[i];
			if (entry.R != i || entry.G != i || entry.B != i) return false;
		}

		return true;
	}

	public static void SetGreyscalePalette([NotNull] Image bitmap)
	{
		if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed) throw new UnsupportedImageFormatException();

		ColorPalette palette = bitmap.Palette;

		for (int i = 0; i < 256; i++)
		{
			palette.Entries[i] = Color.FromArgb(i, i, i);
		}
	}
}