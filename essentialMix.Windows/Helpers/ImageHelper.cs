using System.Drawing;
using essentialMix.Drawing;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Image = System.Drawing.Image;

namespace essentialMix.Windows.Helpers;

public static class ImageHelper
{
	public static Image Crop([NotNull] Image value, Rectangle rectangle, DimensionRestriction restriction = DimensionRestriction.None)
	{
		Bitmap bmp = value as Bitmap;
		bool created = bmp == null;

		try
		{
			if (created) bmp = new Bitmap(value);
			return BitmapHelper.Crop(bmp, rectangle, restriction);
		}
		finally
		{
			if (created) ObjectHelper.Dispose(ref bmp);
		}
	}

	public static Image CropBackground([NotNull] Image value, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
	{
		Bitmap bmp = value as Bitmap;
		bool created = bmp == null;

		try
		{
			if (created) bmp = new Bitmap(value);
			return BitmapHelper.CropBackground(bmp, restriction, unitWidthLimit, unitHeightLimit);
		}
		finally
		{
			if (created) ObjectHelper.Dispose(ref bmp);
		}
	}

	public static Image CropBackground([NotNull] Image value, Color backColor, DimensionRestriction restriction = DimensionRestriction.None, int unitWidthLimit = 0, int unitHeightLimit = 0)
	{
		Bitmap bmp = value as Bitmap;
		bool created = bmp == null;

		try
		{
			if (created) bmp = new Bitmap(value);
			return BitmapHelper.CropBackground(bmp, backColor, restriction, unitWidthLimit, unitHeightLimit);
		}
		finally
		{
			if (created) ObjectHelper.Dispose(ref bmp);
		}
	}
}