using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using essentialMix.Collections;
using essentialMix.Drawing;
using essentialMix.Drawing.Helpers;
using essentialMix.Extensions;
using essentialMix.Helpers;

namespace essentialMix.Media.ffmpeg.Commands;

public sealed class ThumbnailCommand : InputOutputCommand
{
	//http://www.ffmpeg.org/ffmpeg-filters.html#blackdetect
	//https://superuser.com/questions/538112/meaningful-thumbnails-for-a-video-using-ffmpeg
	//https://www.binpress.com/tutorial/how-to-generate-video-previews-with-ffmpeg/138
	//https://stackoverflow.com/questions/28519403/ffmpeg-command-issue
	//https://stackoverflow.com/questions/43119069/ffmpeg-generate-thumbnail-efficiently

	//ffmpeg -y -i FILENAME.mov -ss 00:00:10.00 -vf "select=gt(scene\,0.4),scale=-1:135" -frames:v 1 -vsync vfr FILENAME.jpg
	//ffmpeg -y -i FILENAME.mov -ss 00:00:10.00 -vf "select=gt(scene\,0.4),scale=-1:135" -frames:v 1 -vsync vfr -vf fps=fps=1/600 FILENAME.jpg

	//ffmpeg -y -i FILENAME.mov -vf "blackdetect=d=1:pic_th=0.70:pix_th=0.10,select=gt(scene\,0.25),scale=-1:135,tile=100x1" -frames:v 1 -q:v 1 -vsync vfr FILENAME.jpg
	//ffmpeg -nostdin -loglevel panic -hide_banner -y -i "filename.mp4" -frames 1 -vf "select=eq(pict_type\,I),scale=-1:120,tile=100x1" -vsync vfr "preview.jpg"
	public ThumbnailCommand()
		: base(Properties.Settings.Default.FFMPEG_NAME)
	{
		Arguments.Insert(0, new Property("default", "-nostdin -loglevel panic -hide_banner", true, true));
		Arguments.Insert(2, new Property("seek", "-ss {0}", true, true));
		Arguments.Add(new Property("filter", "-vf \"blackdetect=d=1:pic_th=0.70:pix_th=0.10{0}\"", true, true));
		Arguments.Add(new Property("frames", "-frames:v {0}", true, true));
		Arguments.Add(new Property("variations", "-vsync 0", true, true));
		Arguments.Add(new Property("other", "-an -f image2", true, true));
	}

	public TimeSpan Seek { get; set; }
	public int Frames { get; set; } = 1;
	public int Width { get; set; } = -1;
	public int Height { get; set; } = -1;
	public bool PreviewMode { get; set; }
	public float SceneFilter { get; set; } = 0.3f;
	public int VideoThumbWidth { get; set; } = -1;

	protected override string CollectArgument(IProperty property)
	{
		switch (property.Name)
		{
			case "seek":
				return PreviewMode || !Seek.IsValid() ? null : string.Format((string)property.Value, Seek.TotalSeconds);
			case "filter":
				int w, h;
				string select;
				string scale;
				string tile;

				if (PreviewMode)
				{
					w = -1;
					h = Height.NotBelow(-1);
					select = ",select=key";
					tile = ",tile=100x1";
				}
				else
				{
					w = Width.NotBelow(-1);
					h = Height.NotBelow(-1);

					float scene = SceneFilter.Within(-1.0f, 1.0f);
					select = scene <= 0.00f ? string.Empty : $",select=gt(scene\\,{scene:F2})";
					tile = string.Empty;
				}

				if (w <= 0) w = -1;
				if (h <= 0) h = -1;
				scale = w > 0 || h > 0 ? $",scale={w}:{h}" : string.Empty;
				return string.Format((string)property.Value, select + scale + tile);
			case "frames":
				return string.Format((string)property.Value, PreviewMode ? 1 : Frames.NotBelow(1));
			case "variations":
			case "other":
				return (string)property.Value;
			case "output":
				if (PreviewMode || Frames < 2) return base.CollectArgument(property);
				if (string.IsNullOrWhiteSpace((string)property.Value)) throw new ArgumentNullException(nameof(property.Value));
				if (string.IsNullOrWhiteSpace(Output)) throw new InvalidOperationException(nameof(Output) + " is missing.");

				string fileName = Path.GetFileNameWithoutExtension(Output);
				string ext = Path.GetExtension(Output);
				if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
				return fileName + "%02d" + ext;
			default:
				return base.CollectArgument(property);
		}
	}

	protected override void OnCompleted()
	{
		base.OnCompleted();
		if (!PreviewMode || !File.Exists(Output)) return;

		Bitmap newBmp;
		int width;

		using (Bitmap bmp = new Bitmap(Output))
		{
			if (bmp.Width > 1 && bmp.Height > 1)
			{
				Color backColor = bmp.GetPixel(bmp.Width - 1, bmp.Height - 1);
				width = bmp.Width;
				newBmp = BitmapHelper.CropBackground(bmp, backColor, DimensionRestriction.KeepHeight, VideoThumbWidth);
			}
			else
			{
				width = 0;
				newBmp = null;
			}
		}

		if (newBmp == null) return;
		if (newBmp.Width < width) newBmp.Save(Output, ImageFormat.Jpeg);
		ObjectHelper.Dispose(ref newBmp);
	}
}