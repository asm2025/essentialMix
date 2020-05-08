using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using asm.Extensions;
using JetBrains.Annotations;
using asm.Drawing.Helpers;
using asm.Exceptions.Drawing;
using asm.Helpers;
using asm.Patterns.Object;

namespace asm.Drawing
{
	/// <summary>
	/// AForge.Imaging.UnmanagedImage
	/// </summary>
	/// <remarks>
	/// <para>The class represents wrapper of an image in unmanaged memory. Using this class
	/// it is possible as to allocate new image in unmanaged memory, as to just wrap provided
	/// pointer to unmanaged memory, where an image is stored.</para>
	/// 
	/// <para>Usage of unmanaged images is mostly beneficial when it is required to apply <b>multiple</b>
	/// image processing routines to a single image. In such scenario usage of .NET managed images
	/// usually leads to worse performance, because each routine needs to lock managed image
	/// before image processing is done and then unlock it after image processing is done. Without
	/// these lock/unlock there is no way to get direct access to managed image's data, which means
	/// there is no way to do fast image processing. So, usage of managed images lead to overhead, which
	/// is caused by locks/unlock. Unmanaged images are represented internally using unmanaged memory
	/// buffer. This means that it is not required to do any locks/unlocks in order to get access to image
	/// data (no overhead).</para>
	/// 
	/// <para>Sample usage:</para>
	/// <code>
	/// // sample 1 - wrapping .NET image into unmanaged without
	/// // making extra copy of image in memory
	/// BitmapData imageData = image.LockBits(
	///     new Rectangle( 0, 0, image.Width, image.Height ),
	///     ImageLockMode.ReadWrite, image.PixelFormat );
	/// 
	/// try
	/// {
	///     UnmanagedImage unmanagedImage = new UnmanagedImage( imageData ) );
	///     // apply several routines to the unmanaged image
	/// }
	/// finally
	/// {
	///     image.UnlockBits( imageData );
	/// }
	/// 
	/// 
	/// // sample 2 - converting .NET image into unmanaged
	/// UnmanagedImage unmanagedImage = UnmanagedImage.FromManagedImage( image );
	/// // apply several routines to the unmanaged image
	/// ...
	/// // convert to managed image if it is required to display it at some point of time
	/// Bitmap managedImage = unmanagedImage.ToManagedImage( );
	/// </code>
	/// </remarks>
	public class UnmanagedImage : Disposable, ICloneable
	{
		private bool _mustBeDisposed;

		public UnmanagedImage([NotNull] BitmapData data)
			: this(data.Scan0, data.Width, data.Height, data.Stride, data.PixelFormat)
		{
		}

		/// <inheritdoc />
		public UnmanagedImage(IntPtr imageData, int width, int height, int stride, PixelFormat pixelFormat)
		{
			ImageData = imageData;
			Width = width;
			Height = height;
			Stride = stride;
			PixelFormat = pixelFormat;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing && (_mustBeDisposed || ImageData != IntPtr.Zero))
			{
				Marshal.FreeHGlobal(ImageData);
				GC.RemoveMemoryPressure(Stride * Height);
				ImageData = IntPtr.Zero;
			}

			base.Dispose(disposing);
		}

		public IntPtr ImageData { get; private set; }
		public int Width { get; }
		public int Height { get; }
		public int Stride { get; }
		public PixelFormat PixelFormat { get; }

		/// <inheritdoc />
		public object Clone()
		{
			IntPtr destination = Marshal.AllocHGlobal(Stride * Height);
			GC.AddMemoryPressure(Stride * Height);
			UnmanagedImage image = new UnmanagedImage(destination, Width, Height, Stride, PixelFormat)
			{
				_mustBeDisposed = true
			};
			MemoryHelper.CopyMemory(ImageData, destination, Stride * Height);
			return image;
		}

		/// <summary>
		/// Copies current unmanaged image to the specified image.
		/// </summary>
		/// <remarks>Size and pixel format of the destination image must be exactly the same.</remarks>
		public unsafe void CopyTo([NotNull] UnmanagedImage destination)
		{
			if (Width != destination.Width || Height != destination.Height || PixelFormat != destination.PixelFormat) throw new ImagePropertiesMismatchException();

			if (Stride == destination.Stride)
			{
				MemoryHelper.CopyMemory(ImageData, destination.ImageData, Stride * Height);
			}
			else
			{
				int count = Stride < destination.Stride ? Stride : destination.Stride;
				byte* source = (byte*)ImageData.ToPointer();
				byte* target = (byte*)destination.ImageData.ToPointer();

				for (int index = 0; index < Height; ++index)
				{
					MemoryHelper.CopyMemory(source, target, count);
					target += destination.Stride;
					source += Stride;
				}
			}
		}

		[NotNull] public Bitmap ToBitmap() { return ToBitmap(true); }

		[NotNull]
		public unsafe Bitmap ToBitmap(bool copy)
		{
			Bitmap bitmap = null;

			try
			{
				if (!copy)
				{
					bitmap = new Bitmap(Width, Height, Stride, PixelFormat, ImageData);
					if (PixelFormat == PixelFormat.Format8bppIndexed) ImageHelper.SetGreyscalePalette(bitmap);
				}
				else
				{
					bitmap = PixelFormat == PixelFormat.Format8bppIndexed
								? BitmapHelper.CreateGreyscaleImage(Width, Height)
								: new Bitmap(Width, Height, PixelFormat);
					BitmapData data = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat);
					int count = Math.Min(Stride, data.Stride);

					try
					{
						byte* source = (byte*)ImageData.ToPointer();
						byte* destination = (byte*)data.Scan0.ToPointer();

						if (Stride != data.Stride)
						{
							for (int i = 0; i < Height; ++i)
							{
								MemoryHelper.CopyMemory(source, destination, count);
								destination += data.Stride;
								source += Stride;
							}
						}
						else
							MemoryHelper.CopyMemory(source, destination, Stride * Height);
					}
					finally
					{
						bitmap.UnlockBits(data);
					}
				}
			}
			catch
			{
				ObjectHelper.Dispose(ref bitmap);
			}

			return bitmap ?? throw new InvalidImagePropertiesException();
		}

		/// <summary>Allocate new image in unmanaged memory.</summary>
		/// <remarks>
		/// <para><note>The method supports only
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format8bppIndexed</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format16bppGrayScale</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format24bppRgb</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format32bppRgb</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format32bppArgb</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format32bppPArgb</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format48bppRgb</see>,
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format64bppArgb</see> and
		/// <see cref="T:System.Drawing.Imaging.PixelFormat">Format64bppPArgb</see> pixel formats.
		/// In the case if <see cref="T:System.Drawing.Imaging.PixelFormat">Format8bppIndexed</see>
		/// format is specified, palette is not not created for the image (supposed that it is
		/// 8 bpp greyscale image).
		/// </note></para>
		/// </remarks>
		/// <exception cref="T:AForge.Imaging.UnsupportedImageFormatException">Unsupported pixel format was specified.</exception>
		/// <exception cref="T:AForge.Imaging.InvalidImagePropertiesException">Invalid image size was specified.</exception>
		[NotNull]
		public static UnmanagedImage Create(int width, int height, PixelFormat pixelFormat)
		{
			int pf;

			switch (pixelFormat)
			{
				case PixelFormat.Format24bppRgb:
					pf = 3;
					break;
				case PixelFormat.Format32bppRgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppArgb:
					pf = 4;
					break;
				case PixelFormat.Format8bppIndexed:
					pf = 1;
					break;
				case PixelFormat.Format16bppGrayScale:
					pf = 2;
					break;
				case PixelFormat.Format48bppRgb:
					pf = 6;
					break;
				case PixelFormat.Format64bppPArgb:
				case PixelFormat.Format64bppArgb:
					pf = 8;
					break;
				default:
					throw new UnsupportedImageFormatException();
			}

			if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
			if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

			int stride = width * pf;
			if (stride % 4 != 0) stride += 4 - stride % 4;
			IntPtr ptr = Marshal.AllocHGlobal(stride * height);
			MemoryHelper.SetMemory(ptr, byte.MinValue, stride * height);
			GC.AddMemoryPressure(stride * height);
			return new UnmanagedImage(ptr, width, height, stride, pixelFormat)
			{
				_mustBeDisposed = true
			};
		}

		[NotNull]
		public static UnmanagedImage FromBitmap([NotNull] Bitmap bitmap)
		{
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

			try
			{
				return FromBitmapData(bitmapData);
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}

		[NotNull]
		public static UnmanagedImage FromBitmapData([NotNull] BitmapData data)
		{
			switch (data.PixelFormat)
			{
				case PixelFormat.Format24bppRgb:
				case PixelFormat.Format32bppRgb:
				case PixelFormat.Format8bppIndexed:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format16bppGrayScale:
				case PixelFormat.Format48bppRgb:
				case PixelFormat.Format64bppPArgb:
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format64bppArgb:
					IntPtr ptr = Marshal.AllocHGlobal(data.Stride * data.Height);
					GC.AddMemoryPressure(data.Stride * data.Height);
					UnmanagedImage unmanagedImage = new UnmanagedImage(ptr, data.Width, data.Height, data.Stride, data.PixelFormat)
					{
						_mustBeDisposed = true
					};
					MemoryHelper.CopyMemory(data.Scan0, ptr, data.Stride * data.Height);
					return unmanagedImage;
				default:
					throw new UnsupportedImageFormatException();
			}
		}



		/// <summary>
		/// Collect pixel values from the specified list of coordinates.
		/// </summary>
		/// 
		/// <param name="points">List of coordinates to collect pixels' value from.</param>
		/// 
		/// <returns>Returns array of pixels' values from the specified coordinates.</returns>
		/// 
		/// <remarks><para>The method goes through the specified list of points and for each point retrieves
		/// corresponding pixel's value from the unmanaged image.</para>
		/// 
		/// <para><note>For greyscale image the output array has the same length as number of points in the
		/// specified list of points. For color image the output array has triple length, containing pixels'
		/// values in RGB order.</note></para>
		/// 
		/// <para><note>The method does not make any checks for valid coordinates and leaves this up to user.
		/// If specified coordinates are out of image's bounds, the result is not predictable (crash in most cases).
		/// </note></para>
		/// 
		/// <para><note>This method is supposed for images with 8 bpp channels only (8 bpp greyscale image and
		/// 24/32 bpp color images).</note></para>
		/// </remarks>
		/// 
		/// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the source image. Use Collect16bppPixelValues() method for
		/// images with 16 bpp channels.</exception>
		/// 
		[NotNull]
		public byte[] Collect8bppPixelValues([NotNull] IReadOnlyCollection<Point> points)
		{
			int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;
			if (PixelFormat == PixelFormat.Format16bppGrayScale || pixelSize > 4) throw new UnsupportedImageFormatException();

			byte[] pixelValues = new byte[points.Count * (PixelFormat == PixelFormat.Format8bppIndexed ? 1 : 3)];

			unsafe
			{
				byte* basePtr = (byte*)ImageData.ToPointer();
				byte* ptr;

				if (PixelFormat == PixelFormat.Format8bppIndexed)
				{
					int i = 0;

					foreach (Point point in points)
					{
						ptr = basePtr + Stride * point.Y + point.X;
						pixelValues[i++] = *ptr;
					}
				}
				else
				{
					int i = 0;

					foreach (Point point in points)
					{
						ptr = basePtr + Stride * point.Y + point.X * pixelSize;
						pixelValues[i++] = ptr[Rgb.R];
						pixelValues[i++] = ptr[Rgb.G];
						pixelValues[i++] = ptr[Rgb.B];
					}
				}
			}

			return pixelValues;
		}

		/// <summary>
		/// Collect coordinates of none black pixels in the image.
		/// </summary>
		/// 
		/// <returns>Returns list of points, which have other than black color.</returns>
		/// 
		[NotNull]
		public IList<Point> CollectActivePixels()
		{
			return CollectActivePixels(new Rectangle(0, 0, Width, Height));
		}

		/// <summary>
		/// Collect coordinates of none black pixels within specified rectangle of the image.
		/// </summary>
		/// 
		/// <param name="rect">Image's rectangle to process.</param>
		/// 
		/// <returns>Returns list of points, which have other than black color.</returns>
		///
		[NotNull]
		public IList<Point> CollectActivePixels(Rectangle rect)
		{
			IList<Point> pixels = new List<Point>();
			int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;
			rect.Intersect(new Rectangle(0, 0, Width, Height));

			int startX = rect.X;
			int startY = rect.Y;
			int stopX = rect.Right;
			int stopY = rect.Bottom;

			unsafe
			{
				byte* basePtr = (byte*)ImageData.ToPointer();

				if (PixelFormat == PixelFormat.Format16bppGrayScale || pixelSize > 4)
				{
					int pixelWords = pixelSize >> 1;

					for (int y = startY; y < stopY; y++)
					{
						ushort* ptr = (ushort*)(basePtr + y * Stride + startX * pixelSize);

						if (pixelWords == 1)
						{
							// greyscale images
							for (int x = startX; x < stopX; x++, ptr++)
							{
								if (*ptr != 0) pixels.Add(new Point(x, y));
							}
						}
						else
						{
							// color images
							for (int x = startX; x < stopX; x++, ptr += pixelWords)
							{
								if (ptr[Rgb.R] != 0 || ptr[Rgb.G] != 0 || ptr[Rgb.B] != 0) pixels.Add(new Point(x, y));
							}
						}
					}
				}
				else
				{
					for (int y = startY; y < stopY; y++)
					{
						byte* ptr = basePtr + y * Stride + startX * pixelSize;

						if (pixelSize == 1)
						{
							// greyscale images
							for (int x = startX; x < stopX; x++, ptr++)
							{
								if (*ptr != 0) pixels.Add(new Point(x, y));
							}
						}
						else
						{
							// color images
							for (int x = startX; x < stopX; x++, ptr += pixelSize)
							{
								if (ptr[Rgb.R] != 0 || ptr[Rgb.G] != 0 || ptr[Rgb.B] != 0)
								{
									pixels.Add(new Point(x, y));
								}
							}
						}
					}
				}
			}

			return pixels;
		}

		/// <summary>
		/// Set pixels with the specified coordinates to the specified color.
		/// </summary>
		/// 
		/// <param name="coordinates">List of points to set color for.</param>
		/// <param name="color">Color to set for the specified points.</param>
		/// 
		/// <remarks><para><note>For images having 16 bpp per color plane, the method extends the specified color
		/// value to 16 bit by multiplying it by 256.</note></para></remarks>
		///
		public void SetPixels([NotNull] IReadOnlyCollection<Point> coordinates, Color color)
		{
			unsafe
			{
				int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;
				byte* basePtr = (byte*)ImageData.ToPointer();
				byte red = color.R;
				byte green = color.G;
				byte blue = color.B;
				byte alpha = color.A;

				switch (PixelFormat)
				{
					case PixelFormat.Format8bppIndexed:
						byte greyByte = (byte)(0.2125 * red + 0.7154 * green + 0.0721 * blue);

						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								byte* ptr = basePtr + point.Y * Stride + point.X;
								*ptr = greyByte;
							}
						}

						break;

					case PixelFormat.Format24bppRgb:
					case PixelFormat.Format32bppRgb:
						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								byte* ptr = basePtr + point.Y * Stride + point.X * pixelSize;
								ptr[Rgb.R] = red;
								ptr[Rgb.G] = green;
								ptr[Rgb.B] = blue;
							}
						}
						break;
					case PixelFormat.Format32bppArgb:
						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								byte* ptr = basePtr + point.Y * Stride + point.X * pixelSize;
								ptr[Rgb.R] = red;
								ptr[Rgb.G] = green;
								ptr[Rgb.B] = blue;
								ptr[Rgb.A] = alpha;
							}
						}
						break;
					case PixelFormat.Format16bppGrayScale:
						ushort greyShort = (ushort)((ushort)(0.2125 * red + 0.7154 * green + 0.0721 * blue) << 8);

						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								ushort* ptr = (ushort*)(basePtr + point.Y * Stride) + point.X;
								*ptr = greyShort;
							}
						}
						break;
					case PixelFormat.Format48bppRgb:
						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								ushort* ptr = (ushort*)(basePtr + point.Y * Stride + point.X * pixelSize);
								ptr[Rgb.R] = (ushort)(red << 8);
								ptr[Rgb.G] = (ushort)(green << 8);
								ptr[Rgb.B] = (ushort)(blue << 8);
							}
						}
						break;
					case PixelFormat.Format64bppArgb:
						foreach (Point point in coordinates)
						{
							if (point.X >= 0 && point.Y >= 0 && point.X < Width && point.Y < Height)
							{
								ushort* ptr = (ushort*)(basePtr + point.Y * Stride + point.X * pixelSize);
								ptr[Rgb.R] = (ushort)(red << 8);
								ptr[Rgb.G] = (ushort)(green << 8);
								ptr[Rgb.B] = (ushort)(blue << 8);
								ptr[Rgb.A] = (ushort)(alpha << 8);
							}
						}
						break;
					default:
						throw new UnsupportedImageFormatException();
				}
			}
		}

		public void SetPixel(Point point, Color color)
		{
			SetPixel(point.X, point.Y, color);
		}

		public void SetPixel(int x, int y, Color color)
		{
			SetPixel(x, y, color.R, color.G, color.B, color.A);
		}

		public void SetPixel(int x, int y, byte value)
		{
			SetPixel(x, y, value, value, value, 255);
		}

		private void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
		{
			if (x >= 0 && y >= 0 && x < Width && y < Height)
			{
				unsafe
				{
					int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;
					byte* ptr = (byte*)ImageData.ToPointer() + y * Stride + x * pixelSize;
					ushort* ptr2 = (ushort*)ptr;

					switch (PixelFormat)
					{
						case PixelFormat.Format8bppIndexed:
							*ptr = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
							break;
						case PixelFormat.Format24bppRgb:
						case PixelFormat.Format32bppRgb:
							ptr[Rgb.R] = r;
							ptr[Rgb.G] = g;
							ptr[Rgb.B] = b;
							break;
						case PixelFormat.Format32bppArgb:
							ptr[Rgb.R] = r;
							ptr[Rgb.G] = g;
							ptr[Rgb.B] = b;
							ptr[Rgb.A] = a;
							break;
						case PixelFormat.Format16bppGrayScale:
							*ptr2 = (ushort)((ushort)(0.2125 * r + 0.7154 * g + 0.0721 * b) << 8);
							break;
						case PixelFormat.Format48bppRgb:
							ptr2[Rgb.R] = (ushort)(r << 8);
							ptr2[Rgb.G] = (ushort)(g << 8);
							ptr2[Rgb.B] = (ushort)(b << 8);
							break;
						case PixelFormat.Format64bppArgb:
							ptr2[Rgb.R] = (ushort)(r << 8);
							ptr2[Rgb.G] = (ushort)(g << 8);
							ptr2[Rgb.B] = (ushort)(b << 8);
							ptr2[Rgb.A] = (ushort)(a << 8);
							break;
						default:
							throw new UnsupportedImageFormatException();
					}
				}
			}
		}

		public Color GetPixel(Point point)
		{
			return GetPixel(point.X, point.Y);
		}

		public Color GetPixel(int x, int y)
		{
			if (!x.InRange(0, Width))
				throw new ArgumentOutOfRangeException(nameof(x));

			if (!y.InRange(0, Height))
				throw new ArgumentOutOfRangeException(nameof(y));

			Color color;

			unsafe
			{
				int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;
				byte* ptr = (byte*)ImageData.ToPointer() + y * Stride + x * pixelSize;

				switch (PixelFormat)
				{
					case PixelFormat.Format8bppIndexed:
						color = Color.FromArgb(*ptr, *ptr, *ptr);
						break;
					case PixelFormat.Format24bppRgb:
					case PixelFormat.Format32bppRgb:
						color = Color.FromArgb(ptr[Rgb.R], ptr[Rgb.G], ptr[Rgb.B]);
						break;
					case PixelFormat.Format32bppArgb:
						color = Color.FromArgb(ptr[Rgb.A], ptr[Rgb.R], ptr[Rgb.G], ptr[Rgb.B]);
						break;
					default:
						throw new UnsupportedImageFormatException();
				}
			}

			return color;
		}

		[NotNull]
		public ushort[] Collect16bppPixelValues([NotNull] List<Point> points)
		{
			int pixelSize = Image.GetPixelFormatSize(PixelFormat) / 8;

			if (PixelFormat == PixelFormat.Format8bppIndexed || pixelSize == 3 || pixelSize == 4)
				throw new UnsupportedImageFormatException("Unsupported pixel format of the source image. Use Collect8bppPixelValues() method for it.");

			ushort[] pixelValues = new ushort[points.Count * (PixelFormat == PixelFormat.Format16bppGrayScale ? 1 : 3)];

			unsafe
			{
				byte* basePtr = (byte*)ImageData.ToPointer();
				ushort* ptr;

				if (PixelFormat == PixelFormat.Format16bppGrayScale)
				{
					int i = 0;

					foreach (Point point in points)
					{
						ptr = (ushort*)(basePtr + Stride * point.Y + point.X * pixelSize);
						pixelValues[i++] = *ptr;
					}
				}
				else
				{
					int i = 0;

					foreach (Point point in points)
					{
						ptr = (ushort*)(basePtr + Stride * point.Y + point.X * pixelSize);
						pixelValues[i++] = ptr[Rgb.R];
						pixelValues[i++] = ptr[Rgb.G];
						pixelValues[i++] = ptr[Rgb.B];
					}
				}
			}

			return pixelValues;
		}
	}
}