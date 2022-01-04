using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace essentialMix.Core.WPF.Helpers;

public static class ImageSourceHelper
{
	private static readonly ConcurrentDictionary<SHSTOCKICONID, ImageSource> __stockIconCache = new ConcurrentDictionary<SHSTOCKICONID, ImageSource>();

	public static ImageSource FomMessageBoxImage(MessageBoxImage image)
	{
		if (image == MessageBoxImage.None) return null;
		SHSTOCKICONID id = image switch
		{
			MessageBoxImage.Error => SHSTOCKICONID.SIID_ERROR,
			MessageBoxImage.Question => SHSTOCKICONID.SIID_HELP,
			MessageBoxImage.Warning => SHSTOCKICONID.SIID_WARNING,
			MessageBoxImage.Information => SHSTOCKICONID.SIID_INFO,
			_ => throw new ArgumentOutOfRangeException(nameof(image))
		};
		return FromSystem(id);
	}

	public static ImageSource FromSystem(SHSTOCKICONID id)
	{
		if (!Enum.IsDefined(typeof(SHSTOCKICONID), id)) throw new ArgumentOutOfRangeException(nameof(id));
		return __stockIconCache.GetOrAdd(id, k =>
		{
			SHSTOCKICONINFO sii = new SHSTOCKICONINFO
			{
				cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO))
			};

			int hr = Win32.SHGetStockIconInfo(k, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON, ref sii);

			if (ResultCom.Succeeded(hr))
			{
				ImageSource img = Imaging.CreateBitmapSourceFromHIcon(sii.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				Win32.DestroyIcon(sii.hIcon);
				return img;
			}

			// try to get it from SystemIcons
			Icon icon = k switch
			{
				SHSTOCKICONID.SIID_APPLICATION => SystemIcons.Application,
				SHSTOCKICONID.SIID_INFO => SystemIcons.Information,
				SHSTOCKICONID.SIID_ERROR => SystemIcons.Error,
				SHSTOCKICONID.SIID_WARNING => SystemIcons.Warning,
				SHSTOCKICONID.SIID_HELP => SystemIcons.Question,
				SHSTOCKICONID.SIID_SHIELD => SystemIcons.Shield,
				_ => null
			};

			if (icon == null)
			{
				Marshal.ThrowExceptionForHR(hr);
				throw new ArgumentOutOfRangeException(nameof(id));
			}
			return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
		});
	}
}