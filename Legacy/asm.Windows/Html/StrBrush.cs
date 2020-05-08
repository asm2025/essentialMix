using System;
using System.Drawing;
using JetBrains.Annotations;

namespace asm.Windows.Html
{
	/// <summary>
	/// Represent the value of C# Brush object.
	/// </summary>
	/// <remarks>
	/// This is because if I create an object of Brush class then C# really creates a
	/// brush (which is time expensive), but with the use of this class I can represent
	/// the brush without really creating it.
	/// </remarks>
	public class StrBrush : IComparable
	{
		public StrBrush() { Color = Color.Black; }

		public StrBrush([NotNull] StrBrush oldBrush) { Color = oldBrush.Color; }

		public StrBrush(Color color) { Color = color; }

		public override string ToString() { return Color.ToString(); }

		public Color Color { get; set; }

		[NotNull] public Brush GetRealBrush() { return new SolidBrush(Color); }

		public int CompareTo(object obj)
		{
			return obj is StrBrush old && old.Color == Color
						? 0
						: -1;
		}
	}
}