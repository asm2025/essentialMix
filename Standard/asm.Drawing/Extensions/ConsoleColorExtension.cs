using System;
using System.Drawing;
using asm.Extensions;

namespace asm.Drawing.Extensions
{
	public static class ConsoleColorExtension
	{
		public static Color ToColor(this ConsoleColor thisValue) { return Color.FromName(thisValue.GetName()); }
	}
}