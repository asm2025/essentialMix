using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using JetBrains.Annotations;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace essentialMix.Drawing;

[Serializable]
public struct Offset : IEquatable<Offset>
{
	public Offset(Point location, Point offset)
		: this(location.Y, offset.X, offset.Y, location.X)
	{
	}

	public Offset(Rectangle rectangle)
		: this(rectangle.Top, rectangle.Right, rectangle.Bottom, rectangle.Left)
	{
	}

	public Offset(int top, int right, int bottom, int left)
	{
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}

	public int Top { get; set; }
	public int Right { get; set; }
	public int Bottom { get; set; }
	public int Left { get; set; }

	public static bool operator ==(Offset x, Offset y) { return x.Equals(y); }

	public static bool operator !=(Offset x, Offset y) { return !x.Equals(y); }

	public override bool Equals(object obj) { return obj is Offset offset && Equals(offset); }

	[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 397;
			hash = (hash * 397) ^ Top;
			hash = (hash * 397) ^ Right;
			hash = (hash * 397) ^ Bottom;
			hash = (hash * 397) ^ Left;
			return hash;
		}
	}

	[NotNull]
	public override string ToString() { return $"{Top},{Right},{Bottom},{Left}"; }

	public bool Equals(Offset other) { return other.Top == Top && other.Right == Right && other.Bottom == Bottom && other.Left == Left; }

	public bool IsEmpty() { return Top == 0 && Right == 0 && Bottom == 0 && Left == 0; }

	public bool IsRectangle() { return Top == Right && Right == Bottom && Bottom == Left; }

	public bool IsEqualVertically() { return Top == Bottom; }

	public bool IsEqualHorizontally() { return Left == Right; }
}