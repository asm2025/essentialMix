using System;
using System.Drawing;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class ConsoleColorExtension
	{
		public static Color ToColor(this ConsoleColor thisValue) { return Color.FromName(thisValue.GetName()); }
	}
}