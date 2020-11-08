using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class IWin32WindowExtension
	{
		public static void Activate([NotNull] this IWin32Window thisValue) { Win32.SetActiveWindow(thisValue.Handle); }

		public static void Capture([NotNull] this IWin32Window thisValue, bool value)
		{
			if (value)
			{
				Win32.SetCapture(thisValue.Handle);
				return;
			}

			if (thisValue.HasCapture()) return;
			Win32.ReleaseCapture();
		}

		public static bool HasCapture([NotNull] this IWin32Window thisValue)
		{
			if (thisValue.Handle == IntPtr.Zero) return false;
			IntPtr hWnd = Win32.GetCapture();
			return hWnd == thisValue.Handle;
		}

		public static bool GetBounds([NotNull] this IWin32Window thisValue, out Rectangle rectangle)
		{
			rectangle = Rectangle.Empty;
			if (!Win32.GetWindowRect(new HandleRef(thisValue, thisValue.Handle), out RECT rct)) return false;
			rectangle = rct.ToRectangle();
			return true;
		}

		public static bool SetBounds([NotNull] this IWin32Window thisValue, Rectangle rectangle, bool bRepaint = true)
		{
			return Win32.MoveWindow(thisValue.Handle, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, bRepaint);
		}

		public static Point MapPointToClient([NotNull] this IWin32Window thisValue, Point pt) { return MapPoint(null, pt, thisValue); }

		public static Point MapPoint(this IWin32Window thisValue, Point pt, IWin32Window newWin = null)
		{
			POINT pnt = pt.ToWin32Point();
			Win32.MapWindowPoints(thisValue?.Handle ?? IntPtr.Zero, newWin?.Handle ?? IntPtr.Zero, ref pnt, 1);
			return pnt.ToPoint();
		}

		public static Rectangle MapRectangle([NotNull] this IWin32Window thisValue, Rectangle rectangle, IWin32Window newWin = null)
		{
			RECT ir = rectangle.ToWin32Rect();
			Win32.MapWindowPoints(thisValue.Handle, newWin?.Handle ?? IntPtr.Zero, ref ir, 2);
			return ir.ToRectangle();
		}

		[NotNull]
		public static Point[] MapPoints([NotNull] this IWin32Window thisValue, [NotNull] Point[] points, IWin32Window newWin = null)
		{
			IntPtr hWndTo = newWin?.Handle ?? IntPtr.Zero;
			POINT[] pts = new POINT[points.Length];

			for (int i = 0; i < pts.Length; i++)
			{
				pts[i] = points[i].ToWin32Point();
				Win32.MapWindowPoints(thisValue.Handle, hWndTo, ref pts[i], 1);
			}

			return pts.Select(e => e.ToPoint()).ToArray();
		}

		public static Point PointToScreen([NotNull] this IWin32Window thisValue, Point p)
		{
			POINT pt = p.ToWin32Point();
			Win32.MapWindowPoints(thisValue.Handle, IntPtr.Zero, ref pt, 1);
			return pt.ToPoint();
		}

		public static void SetPos([NotNull] this IWin32Window thisValue, Rectangle rectangle, IntPtr hWndInsertAfter, WindowPositionFlagsEnum flags)
		{
			Win32.SetWindowPos(thisValue.Handle, hWndInsertAfter, rectangle.Left, rectangle.Top, rectangle.Width + 16, rectangle.Height + 38, flags);
		}
	}
}