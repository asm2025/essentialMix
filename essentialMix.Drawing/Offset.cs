using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using JetBrains.Annotations;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;

namespace essentialMix.Drawing;

[Serializable]
public struct Offset(int top, int right, int bottom, int left)
	: IEquatable<Offset>
{
	public Offset(Point location, Point offset)
		: this(location.Y, offset.X, offset.Y, location.X)
	{
	}

	public Offset(Rectangle rectangle)
		: this(rectangle.Top, rectangle.Right, rectangle.Bottom, rectangle.Left)
	{
	}

	public int Top { get; set; } = top;
	public int Right { get; set; } = right;
	public int Bottom { get; set; } = bottom;
	public int Left { get; set; } = left;

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