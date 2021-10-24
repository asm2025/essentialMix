using System;

namespace essentialMix.Media
{
	/*
	 * https://en.wikipedia.org/wiki/Graphics_display_resolution
	 * https://en.wikipedia.org/wiki/List_of_common_resolutions
	 * https://support.google.com/youtube/answer/2853702?hl=en
	 * https://www.ffmpeg.org/ffmpeg-utils.html#Video-size
	 * https://lists.ffmpeg.org/pipermail/ffmpeg-devel/2007-June/029982.html
	 * https://gist.github.com/atenni/5e1496fd254c3dd8503dee3a6936eb53
	 * https://en.wikipedia.org/wiki/FFmpeg
	*/
	public enum VideoSizeEnum
	{
		Empty = -2,
		Custom = -1,
		Automatic,
		SQCIF,

		QQVGA,
		HQVGA120,

		QCIF, //144

		HQVGA160,

		CGA,

		WQVGAa,

		QVGA,
		FILM,
		WQVGA240_360,
		WQVGA240_384,
		WQVGA240_400,
		FWQVGA, //240
		HVGA240,

		CIF,

		qSVGA,

		HVGA320, //320p

		EGA,

		nHD, //360p

		VGA,
		WVGA480_720, //480p
		WVGA480_768,
		WVGA480_800,
		WVGA480_852,
		FWVGA,

		qHD,

		CIF4,
		PAL, //576p
		SPAL,
		WSVGA576,

		SVGA,
		WSVGA600,

		DVGA,

		WXGAH, //HD720

		XGA,
		WXGA1152,
		WXGA1280,
		FWXGA,

		WXGA,

		Scope2K,

		XGAp,

		WXGAp,
		HDp,
		CWSXGA,

		SXGAm,
		FWXGAp,

		SXGA,
		WSXGA,

		SXGAp,
		WSXGAp,

		FHD, //HD1080
		UHD2KFLAT, //2K
		UHD2K,
		UW_UXGA,

		CIF16,
		QWXGA,

		UXGA,
		WUXGA,

		FHDp1280,

		TXGA,

		FHDp1440,
		WQHD,
		UWQHD,

		QXGA,

		WQXGA,
		UWQHDp,

		Scope4K,

		WQXGAp,

		QSXGA,
		WQSXGA,

		QSXGAp,

		UHD4K,
		Flat4K,
		DCI4K, //4K
		UW5K,

		QUXGA,
		WQUXGA,

		UHDp, //5K

		HXGA,

		WHXGA,

		HSXGA,
		WHSXGA,

		UHD8K, //8K
		UW10K,

		HUXGA,
		WHUXGA,

		UHD10K, //10K

		Fulldome8K
	}

	public static class VideoSizeEnumExtension
	{
		public static (int, int) Dimensions(this VideoSizeEnum thisValue)
		{
			return thisValue switch
			{
				VideoSizeEnum.Empty => (-2, -2),
				VideoSizeEnum.Custom => (-1, -1),
				VideoSizeEnum.Automatic => (0, 0),
				VideoSizeEnum.SQCIF => (128, 96),
				VideoSizeEnum.QQVGA => (160, 120),
				VideoSizeEnum.HQVGA120 => (240, 120),
				VideoSizeEnum.QCIF => (176, 144),
				VideoSizeEnum.HQVGA160 => (240, 160),
				VideoSizeEnum.CGA => (320, 200),
				VideoSizeEnum.WQVGAa => (480, 234),
				VideoSizeEnum.QVGA => (320, 240),
				VideoSizeEnum.FILM => (352, 240),
				VideoSizeEnum.WQVGA240_360 => (360, 240),
				VideoSizeEnum.WQVGA240_384 => (384, 240),
				VideoSizeEnum.WQVGA240_400 => (400, 240),
				VideoSizeEnum.FWQVGA => (432, 240),
				VideoSizeEnum.HVGA240 => (640, 240),
				VideoSizeEnum.CIF => (352, 288),
				VideoSizeEnum.qSVGA => (400, 300),
				VideoSizeEnum.HVGA320 => (480, 320),
				VideoSizeEnum.EGA => (640, 350),
				VideoSizeEnum.nHD => (640, 360),
				VideoSizeEnum.VGA => (640, 480),
				VideoSizeEnum.WVGA480_720 => (720, 480),
				VideoSizeEnum.WVGA480_768 => (768, 480),
				VideoSizeEnum.WVGA480_800 => (800, 480),
				VideoSizeEnum.WVGA480_852 => (852, 480),
				VideoSizeEnum.FWVGA => (854, 480),
				VideoSizeEnum.qHD => (960, 540),
				VideoSizeEnum.CIF4 => (704, 576),
				VideoSizeEnum.PAL => (720, 576),
				VideoSizeEnum.SPAL => (768, 576),
				VideoSizeEnum.WSVGA576 => (1024, 576),
				VideoSizeEnum.SVGA => (800, 600),
				VideoSizeEnum.WSVGA600 => (1024, 600),
				VideoSizeEnum.DVGA => (960, 640),
				VideoSizeEnum.WXGAH => (1280, 720),
				VideoSizeEnum.XGA => (1024, 768),
				VideoSizeEnum.WXGA1152 => (1152, 768),
				VideoSizeEnum.WXGA1280 => (1280, 768),
				VideoSizeEnum.FWXGA => (1366, 768),
				VideoSizeEnum.WXGA => (1280, 800),
				VideoSizeEnum.Scope2K => (2048, 858),
				VideoSizeEnum.XGAp => (1152, 864),
				VideoSizeEnum.WXGAp => (1440, 900),
				VideoSizeEnum.HDp => (1600, 900),
				VideoSizeEnum.CWSXGA => (2880, 900),
				VideoSizeEnum.SXGAm => (1280, 960),
				VideoSizeEnum.FWXGAp => (1440, 960),
				VideoSizeEnum.SXGA => (1280, 1024),
				VideoSizeEnum.WSXGA => (1600, 1024),
				VideoSizeEnum.SXGAp => (1400, 1050),
				VideoSizeEnum.WSXGAp => (1680, 1050),
				VideoSizeEnum.FHD => (1920, 1080),
				VideoSizeEnum.UHD2KFLAT => (1998, 1080),
				VideoSizeEnum.UHD2K => (2048, 1080),
				VideoSizeEnum.UW_UXGA => (2560, 1080),
				VideoSizeEnum.CIF16 => (1408, 1152),
				VideoSizeEnum.QWXGA => (2048, 1152),
				VideoSizeEnum.UXGA => (1600, 1200),
				VideoSizeEnum.WUXGA => (1920, 1200),
				VideoSizeEnum.FHDp1280 => (1920, 1280),
				VideoSizeEnum.TXGA => (1920, 1400),
				VideoSizeEnum.FHDp1440 => (2160, 1440),
				VideoSizeEnum.WQHD => (2560, 1440),
				VideoSizeEnum.UWQHD => (3440, 1440),
				VideoSizeEnum.QXGA => (2048, 1536),
				VideoSizeEnum.WQXGA => (2560, 1600),
				VideoSizeEnum.UWQHDp => (3840, 1600),
				VideoSizeEnum.Scope4K => (4096, 1716),
				VideoSizeEnum.WQXGAp => (3200, 1800),
				VideoSizeEnum.QSXGA => (2560, 2048),
				VideoSizeEnum.WQSXGA => (3200, 2048),
				VideoSizeEnum.QSXGAp => (2800, 2100),
				VideoSizeEnum.UHD4K => (3840, 2160),
				VideoSizeEnum.Flat4K => (3996, 2160),
				VideoSizeEnum.DCI4K => (4096, 2160),
				VideoSizeEnum.UW5K => (5120, 2160),
				VideoSizeEnum.QUXGA => (3200, 2400),
				VideoSizeEnum.WQUXGA => (3840, 2400),
				VideoSizeEnum.UHDp => (5120, 2880),
				VideoSizeEnum.HXGA => (4096, 3072),
				VideoSizeEnum.WHXGA => (5120, 3200),
				VideoSizeEnum.HSXGA => (5120, 4096),
				VideoSizeEnum.WHSXGA => (6400, 4096),
				VideoSizeEnum.UHD8K => (7680, 4320),
				VideoSizeEnum.UW10K => (10240, 4320),
				VideoSizeEnum.HUXGA => (6400, 4800),
				VideoSizeEnum.WHUXGA => (7680, 4800),
				VideoSizeEnum.UHD10K => (10240, 5760),
				VideoSizeEnum.Fulldome8K => (8192, 8192),
				_ => throw new ArgumentOutOfRangeException(nameof(thisValue), thisValue, null)
			};
		}
	}

	public static class VideoSizeEnumHelper
	{
		public static VideoSizeEnum FromDimensions(string value)
		{
			if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
			
			int n = value.IndexOf('x');

			if (n < 0)
			{
				if (!int.TryParse(value, out n)) throw new FormatException($"'{value}' is not recognized as a valid numeric value.");
				return FromDimensions(n, n);
			}

			if (!int.TryParse(value.Substring(0, n), out int x)) throw new FormatException($"'{value}' is not recognized as a valid numeric value.");
			if (!int.TryParse(value.Substring(++n), out int y)) throw new FormatException($"'{value}' is not recognized as a valid numeric value.");
			return FromDimensions(x, y);
		}

		public static VideoSizeEnum FromDimensions(int width, int height)
		{
			if (width == -2 && height == -2) return VideoSizeEnum.Empty;
			if (width == -1 || height == -1) return VideoSizeEnum.Custom;
			if (width == 0 && height == 0) return VideoSizeEnum.Automatic;

			switch (height)
			{
				case 96:
					if (width == 128) return VideoSizeEnum.SQCIF;
					break;
				case 120:
					switch (width)
					{
						case 160:
							return VideoSizeEnum.QQVGA;
						case 240:
							return VideoSizeEnum.HQVGA120;
					}
					break;
				case 144:
					if (width == 176) return VideoSizeEnum.QCIF;
					break;
				case 160:
					if (width == 240) return VideoSizeEnum.HQVGA160;
					break;
				case 200:
					if (width == 320) return VideoSizeEnum.CGA;
					break;
				case 234:
					if (width == 480) return VideoSizeEnum.WQVGAa;
					break;
				case 240:
					switch (width)
					{
						case 320:
							return VideoSizeEnum.QVGA;
						case 352:
							return VideoSizeEnum.FILM;
						case 360:
							return VideoSizeEnum.WQVGA240_360;
						case 384:
							return VideoSizeEnum.WQVGA240_384;
						case 400:
							return VideoSizeEnum.WQVGA240_400;
						case 432:
							return VideoSizeEnum.FWQVGA;
						case 640:
							return VideoSizeEnum.HVGA240;
					}
					break;
				case 288:
					if (width == 352) return VideoSizeEnum.CIF;
					break;
				case 300:
					if (width == 400) return VideoSizeEnum.qSVGA;
					break;
				case 320:
					if (width == 480) return VideoSizeEnum.HVGA320;
					break;
				case 350:
					if (width == 640) return VideoSizeEnum.EGA;
					break;
				case 360:
					if (width == 640) return VideoSizeEnum.nHD;
					break;
				case 480:
					switch (width)
					{
						case 640:
							return VideoSizeEnum.VGA;
						case 720:
							return VideoSizeEnum.WVGA480_720;
						case 768:
							return VideoSizeEnum.WVGA480_768;
						case 800:
							return VideoSizeEnum.WVGA480_800;
						case 852:
							return VideoSizeEnum.WVGA480_852;
						case 854:
							return VideoSizeEnum.FWVGA;
					}
					break;
				case 540:
					if (width == 960) return VideoSizeEnum.qHD;
					break;
				case 576:
					switch (width)
					{
						case 704:
							return VideoSizeEnum.CIF4;
						case 720:
							return VideoSizeEnum.PAL;
						case 768:
							return VideoSizeEnum.SPAL;
						case 1024:
							return VideoSizeEnum.WSVGA576;
					}
					break;
				case 600:
					switch (width)
					{
						case 800:
							return VideoSizeEnum.SVGA;
						case 1024:
							return VideoSizeEnum.WSVGA600;
					}
					break;
				case 640:
					if (width == 960) return VideoSizeEnum.DVGA;
					break;
				case 720:
					if (width == 1280) return VideoSizeEnum.WXGAH;
					break;
				case 768:
					switch (width)
					{
						case 1024:
							return VideoSizeEnum.XGA;
						case 1152:
							return VideoSizeEnum.WXGA1152;
						case 1280:
							return VideoSizeEnum.WXGA1280;
						case 1366:
							return VideoSizeEnum.FWXGA;
					}
					break;
				case 800:
					if (width == 1280) return VideoSizeEnum.WXGA;
					break;
				case 858:
					if (width == 2048) return VideoSizeEnum.Scope2K;
					break;
				case 864:
					if (width == 1152) return VideoSizeEnum.XGAp;
					break;
				case 900:
					switch (width)
					{
						case 1440:
							return VideoSizeEnum.WXGAp;
						case 1600:
							return VideoSizeEnum.HDp;
						case 2880:
							return VideoSizeEnum.CWSXGA;
					}
					break;
				case 960:
					switch (width)
					{
						case 1280:
							return VideoSizeEnum.SXGAm;
						case 1440:
							return VideoSizeEnum.FWXGAp;
					}
					break;
				case 1024:
					switch (width)
					{
						case 1280:
							return VideoSizeEnum.SXGA;
						case 1600:
							return VideoSizeEnum.WSXGA;
					}
					break;
				case 1050:
					switch (width)
					{
						case 1400:
							return VideoSizeEnum.SXGAp;
						case 1680:
							return VideoSizeEnum.WSXGAp;
					}
					break;
				case 1080:
					switch (width)
					{
						case 1920:
							return VideoSizeEnum.FHD;
						case 1998:
							return VideoSizeEnum.UHD2KFLAT;
						case 2048:
							return VideoSizeEnum.UHD2K;
						case 2560:
							return VideoSizeEnum.UW_UXGA;
					}
					break;
				case 1152:
					switch (width)
					{
						case 1408:
							return VideoSizeEnum.CIF16;
						case 2048:
							return VideoSizeEnum.QWXGA;
					}
					break;
				case 1200:
					switch (width)
					{
						case 1600:
							return VideoSizeEnum.UXGA;
						case 1920:
							return VideoSizeEnum.WUXGA;
					}
					break;
				case 1280:
					if (width == 1920) return VideoSizeEnum.FHDp1280;
					break;
				case 1400:
					if (width == 2160) return VideoSizeEnum.FHDp1440;
					break;
				case 1440:
					switch (width)
					{
						case 2560:
							return VideoSizeEnum.WQHD;
						case 3440:
							return VideoSizeEnum.UWQHD;
					}
					break;
				case 1536:
					if (width == 2048) return VideoSizeEnum.QXGA;
					break;
				case 1600:
					switch (width)
					{
						case 2560:
							return VideoSizeEnum.WQXGA;
						case 3840:
							return VideoSizeEnum.UWQHDp;
					}
					break;
				case 1716:
					if (width == 4096) return VideoSizeEnum.Scope4K;
					break;
				case 1800:
					if (width == 3200) return VideoSizeEnum.WQXGAp;
					break;
				case 2048:
					switch (width)
					{
						case 2560:
							return VideoSizeEnum.QSXGA;
						case 3200:
							return VideoSizeEnum.WQSXGA;
					}
					break;
				case 2100:
					if (width == 2800) return VideoSizeEnum.QSXGAp;
					break;
				case 2160:
					switch (width)
					{
						case 3840:
							return VideoSizeEnum.UHD4K;
						case 3996:
							return VideoSizeEnum.Flat4K;
						case 4096:
							return VideoSizeEnum.DCI4K;
						case 5120:
							return VideoSizeEnum.UW5K;
					}
					break;
				case 2400:
					switch (width)
					{
						case 3200:
							return VideoSizeEnum.QUXGA;
						case 3840:
							return VideoSizeEnum.WQUXGA;
					}
					break;
				case 2880:
					if (width == 5120) return VideoSizeEnum.UHDp;
					break;
				case 3072:
					if (width == 4096) return VideoSizeEnum.HXGA;
					break;
				case 3200:
					if (width == 5120) return VideoSizeEnum.WHXGA;
					break;
				case 4096:
					switch (width)
					{
						case 5120:
							return VideoSizeEnum.HSXGA;
						case 6400:
							return VideoSizeEnum.WHSXGA;
					}
					break;
				case 4320:
					switch (width)
					{
						case 7680:
							return VideoSizeEnum.UHD8K;
						case 10240:
							return VideoSizeEnum.UW10K;
					}
					break;
				case 4800:
					switch (width)
					{
						case 6400:
							return VideoSizeEnum.HUXGA;
						case 7680:
							return VideoSizeEnum.WHUXGA;
					}
					break;
				case 5760:
					if (width == 10240) return VideoSizeEnum.UHD10K;
					break;
				case 8192:
					if (width == 8192) return VideoSizeEnum.Fulldome8K;
					break;
			}

			return VideoSizeEnum.Custom;
		}
	}
}