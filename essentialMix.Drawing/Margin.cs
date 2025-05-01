using System;
using System.Drawing;
using JetBrains.Annotations;

namespace essentialMix.Drawing;

[Serializable]
public struct Margin(int top, int right, int bottom, int left)
	: IEquatable<Margin>
{
	private const string TERM = "margin";

	private string _toString = null;
	private int _left = left;
	private int _top = top;
	private int _right = right;
	private int _bottom = bottom;

	public Margin(Point location, Point offset)
		: this(location.Y, offset.X, offset.Y, location.X)
	{
	}

	public Margin(Rectangle rectangle)
		: this(rectangle.Top, rectangle.Right, rectangle.Bottom, rectangle.Left)
	{
	}

	public Margin(Offset offset)
		: this(offset.Top, offset.Right, offset.Bottom, offset.Left)
	{
	}

	public int Top
	{
		get => _top;
		set
		{
			_top = value;
			_toString = null;
		}
	}

	public int Right
	{
		get => _right;
		set
		{
			_right = value;
			_toString = null;
		}
	}

	public int Bottom
	{
		get => _bottom;
		set
		{
			_bottom = value;
			_toString = null;
		}
	}

	public int Left
	{
		get => _left;
		set
		{
			_left = value;
			_toString = null;
		}
	}

	public static bool operator ==(Margin x, Margin y) { return x.Equals(y); }

	public static bool operator !=(Margin x, Margin y) { return !x.Equals(y); }

	public static explicit operator Offset(Margin value) { return new Offset(value.Top, value.Right, value.Bottom, value.Left); }

	public static explicit operator Margin(Offset value) { return new Margin(value); }

	public override bool Equals(object obj)
	{
		switch (obj)
		{
			case null:
				return false;
			case Offset offset:
				return Equals(offset);
			case Margin margin:
				return Equals(margin);
			default:
				return false;
		}
	}

	public override int GetHashCode()
	{
		unchecked // Overflow is fine, just wrap
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
	public override string ToString()
	{
		if (_toString != null) return _toString;

		if (IsRectangle()) _toString = $"{TERM}: {Top}px;";
		else
		{
			bool horizontally = IsEqualHorizontally();
			bool vertically = IsEqualVertically();

			if (horizontally && vertically) _toString = $"{TERM}: {Top}px {Right}px;";
			else if (horizontally) _toString = $"{TERM}: {Top}px {Right}px {Bottom}px;";
			else _toString = $"{TERM}: {Top}px {Right}px {Bottom}px {Left}px;";
		}

		_toString = _toString.Replace("0px", "0");
		return _toString;
	}

	public bool Equals(Offset other) { return other.Top == Top && other.Right == Right && other.Bottom == Bottom && other.Left == Left; }

	public bool Equals(Margin other) { return other.Top == Top && other.Right == Right && other.Bottom == Bottom && other.Left == Left; }

	public bool IsEmpty() { return Top == 0 && Right == 0 && Bottom == 0 && Left == 0; }

	public bool IsRectangle() { return Top == Right && Right == Bottom && Bottom == Left; }

	public bool IsEqualVertically() { return Top == Bottom; }

	public bool IsEqualHorizontally() { return Left == Right; }
}