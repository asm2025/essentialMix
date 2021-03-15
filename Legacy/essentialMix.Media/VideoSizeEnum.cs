using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum VideoSizeEnum
	{
		Empty = -2,
		Custom = -1,
		Automatic,
		[Display(Name = "SQCIF", Description = "128x96", ShortName = "sqcif")] SQCIF,

		[Display(Name = "QQVGA", Description = "160x120", ShortName = "qqvga")] QQVGA,
		[Display(Name = "HQVGA", Description = "240x120")] HQVGA120,

		[Display(Name = "144p", Description = "176x144", ShortName = "qcif")] QCIF, //144

		[Display(Name = "HQVGA", Description = "240x160", ShortName = "hqvga")] HQVGA160,

		[Display(Name = "CGA", Description = "320x200", ShortName = "cga")] CGA,

		[Display(Name = "WQVGA*", Description = "480x234")] WQVGAa,

		[Display(Name = "QVGA", Description = "320x240", ShortName = "qvga")] QVGA,
		[Display(Name = "FILM", Description = "352x240", ShortName = "film")] FILM,
		[Display(Name = "WQVGA", Description = "360x240")] WQVGA240_360,
		[Display(Name = "WQVGA", Description = "384x240")] WQVGA240_384,
		[Display(Name = "WQVGA", Description = "400x240", ShortName = "wqvga")] WQVGA240_400,
		[Display(Name = "240p", Description = "432x240", ShortName = "fwqvga")] FWQVGA, //240
		[Display(Name = "HVGA", Description = "640x240")] HVGA240,

		[Display(Name = "CIF", Description = "352x288", ShortName = "cif")] CIF,

		[Display(Name = "qSVGA", Description = "400x300")] qSVGA,

		[Display(Name = "320p", Description = "480x320", ShortName = "hvga")] HVGA320, //320p

		[Display(Name = "EGA", Description = "640x350", ShortName = "ega")] EGA,

		[Display(Name = "360p", Description = "640x360", ShortName = "nhd")] nHD, //360p

		[Display(Name = "VGA", Description = "640x480", ShortName = "vga")] VGA,
		[Display(Name = "480p", Description = "720x480", ShortName = "ntsc")] WVGA480_720, //480p
		[Display(Name = "WVGA", Description = "768x480")] WVGA480_768,
		[Display(Name = "WVGA", Description = "800x480")] WVGA480_800,
		[Display(Name = "WVGA", Description = "852x480", ShortName = "hd480")] WVGA480_852,
		[Display(Name = "FWVGA", Description = "854x480")] FWVGA,

		[Display(Name = "qHD", Description = "960x540", ShortName = "qhd")] qHD,

		[Display(Name = "4CIF", Description = "704x576", ShortName = "4cif")] CIF4,
		[Display(Name = "576p", Description = "720x576", ShortName = "pal")] PAL, //576p
		[Display(Name = "SPAL", Description = "768x576", ShortName = "spal")] SPAL,
		[Display(Name = "WSVGA", Description = "1024x576")] WSVGA576,

		[Display(Name = "SVGA", Description = "800x600", ShortName = "svga")] SVGA,
		[Display(Name = "WSVGA", Description = "1024x600")] WSVGA600,

		[Display(Name = "DVGA", Description = "960x640")] DVGA,

		[Display(Name = "HD720", Description = "1280x720", ShortName = "hd720")] WXGAH, //HD720

		[Display(Name = "XGA", Description = "1024x768", ShortName = "xga")] XGA,
		[Display(Name = "WXGA", Description = "1152x768")] WXGA1152,
		[Display(Name = "WXGA", Description = "1280x768")] WXGA1280,
		[Display(Name = "FWXGA", Description = "1366x768", ShortName = "wxga")] FWXGA,

		[Display(Name = "WXGA", Description = "1280x800")] WXGA,

		[Display(Name = "2KScope", Description = "2048x858", ShortName = "2kscope")] Scope2K,

		[Display(Name = "XGA+", Description = "1152x864")] XGAp,

		[Display(Name = "WXGA+", Description = "1440x900")] WXGAp,
		[Display(Name = "HD+", Description = "1600x900")] HDp,
		[Display(Name = "CWSXGA", Description = "2880x900")] CWSXGA,

		[Display(Name = "SXGA−", Description = "1280x960")] SXGAm,
		[Display(Name = "FWXGA+", Description = "1440x960")] FWXGAp,

		[Display(Name = "SXGA", Description = "1280x1024", ShortName = "sxga")] SXGA,
		[Display(Name = "WSXGA", Description = "1600x1024", ShortName = "wsxga")] WSXGA,

		[Display(Name = "SXGA+", Description = "1400x1050")] SXGAp,
		[Display(Name = "WSXGA+", Description = "1680x1050")] WSXGAp,

		[Display(Name = "HD1080", Description = "1920x1080", ShortName = "hd1080")] FHD, //HD1080
		[Display(Name = "2K", Description = "2048x1080", ShortName = "2k")] UHD2K, //2K
		[Display(Name = "UHD2KFLAT", Description = "1998x1080", ShortName = "2kflat")] UHD2KFLAT,
		[Display(Name = "UW-UXGA", Description = "2560x1080")] UW_UXGA,

		[Display(Name = "16CIF", Description = "1408x1152", ShortName = "16cif")] CIF16,
		[Display(Name = "QWXGA", Description = "2048x1152")] QWXGA,

		[Display(Name = "UXGA", Description = "1600x1200", ShortName = "uxga")] UXGA,
		[Display(Name = "WUXGA", Description = "1920x1200", ShortName = "wuxga")] WUXGA,

		[Display(Name = "FHD+", Description = "1920x1280")] FHDp1280,

		[Display(Name = "TXGA", Description = "1920x1400")] TXGA,

		[Display(Name = "FHD+", Description = "2160x1440")] FHDp1440,
		[Display(Name = "WQHD", Description = "2560x1440")] WQHD,
		[Display(Name = "UWQHD", Description = "3440x1440")] UWQHD,

		[Display(Name = "QXGA", Description = "2048x1536", ShortName = "qxga")] QXGA,

		[Display(Name = "WQXGA", Description = "2560x1600", ShortName = "woxga")] WQXGA,
		[Display(Name = "UWQHD+", Description = "3840x1600")] UWQHDp,

		[Display(Name = "4KScope", Description = "4096x1716")] Scope4K,

		[Display(Name = "WQXGA+", Description = "3200x1800")] WQXGAp,

		[Display(Name = "QSXGA", Description = "2560x2048", ShortName = "qsxga")] QSXGA,
		[Display(Name = "WQSXGA", Description = "3200x2048", ShortName = "wqsxga")] WQSXGA,

		[Display(Name = "QSXGA+", Description = "2800x2100")] QSXGAp,

		[Display(Name = "UHD4K", Description = "3840x2160", ShortName = "uhd2160")] UHD4K,
		[Display(Name = "4KFlat", Description = "3996x2160", ShortName = "4kflat")] Flat4K,
		[Display(Name = "4K", Description = "4096x2160", ShortName = "4k")] DCI4K, //4K
		[Display(Name = "UW5K", Description = "5120x2160")] UW5K,

		[Display(Name = "QUXGA", Description = "3200x2400")] QUXGA,
		[Display(Name = "WQUXGA", Description = "3840x2400", ShortName = "wquxga")] WQUXGA,

		[Display(Name = "5K", Description = "5120x2880")] UHDp, //5K

		[Display(Name = "HXGA", Description = "4096x3072")] HXGA,

		[Display(Name = "WHXGA", Description = "5120x3200")] WHXGA,

		[Display(Name = "HSXGA", Description = "5120x4096", ShortName = "hsxga")] HSXGA,
		[Display(Name = "WHSXGA", Description = "6400x4096", ShortName = "whsxga")] WHSXGA,

		[Display(Name = "8K", Description = "7680x4320", ShortName = "uhd4320")] UHD8K, //8K
		[Display(Name = "UW10K", Description = "10240x4320")] UW10K,

		[Display(Name = "HUXGA", Description = "6400x4800")] HUXGA,
		[Display(Name = "WHUXGA", Description = "7680x4800", ShortName = "whuxga")] WHUXGA,

		[Display(Name = "10K", Description = "10240x5760")] UHD10K, //10K

		[Display(Name = "Fulldome8K", Description = "8192x8192")] Fulldome8K
	}
}