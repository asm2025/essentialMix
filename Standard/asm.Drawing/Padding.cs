using System;
using System.Drawing;
using JetBrains.Annotations;

namespace asm.Drawing
{
	[Serializable]
	public struct Padding : IEquatable<Padding>
	{
		private const string TERM = "padding";

		private string _toString;
		private int _left;
		private int _top;
		private int _right;
		private int _bottom;

		public Padding(Point location, Point offset)
			: this(location.Y, offset.X, offset.Y, location.X)
		{
		}

		public Padding(Rectangle rectangle)
			: this(rectangle.Top, rectangle.Right, rectangle.Bottom, rectangle.Left)
		{
		}

		public Padding(Offset offset)
			: this(offset.Top, offset.Right, offset.Bottom, offset.Left)
		{
		}

		public Padding(int top, int right, int bottom, int left) 
		{
			_top = top;
			_right = right;
			_bottom = bottom;
			_left = left;
			_toString = null;
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

		public static bool operator ==(Padding x, Padding y) { return x.Equals(y); }

		public static bool operator !=(Padding x, Padding y) { return !x.Equals(y); }

		public static explicit operator Offset(Padding value) { return new Offset(value.Top, value.Right, value.Bottom, value.Left); }

		public static explicit operator Padding(Offset value) { return new Padding(value); }

		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case null:
					return false;
				case Offset offset:
					return Equals(offset);
				case Padding padding:
					return Equals(padding);
				default:
					return false;
			}
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				hash = hash * 29 + Top;
				hash = hash * 29 + Right;
				hash = hash * 29 + Bottom;
				hash = hash * 29 + Left;
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
				bool horz = IsEqualHorizontally();
				bool vert = IsEqualVertically();

				if (horz && vert) _toString = $"{TERM}: {Top}px {Right}px;";
				else if (horz) _toString = $"{TERM}: {Top}px {Right}px {Bottom}px;";
				else _toString = $"{TERM}: {Top}px {Right}px {Bottom}px {Left}px;";
			}

			_toString = _toString.Replace("0px", "0");
			return _toString;
		}

		public bool Equals(Offset other) { return other.Top == Top && other.Right == Right && other.Bottom == Bottom && other.Left == Left; }

		public bool Equals(Padding other) { return other.Top == Top && other.Right == Right && other.Bottom == Bottom && other.Left == Left; }

		public bool IsEmpty() { return Top == 0 && Right == 0 && Bottom == 0 && Left == 0; }

		public bool IsRectangle() { return Top == Right && Right == Bottom && Bottom == Left; }

		public bool IsEqualVertically() { return Top == Bottom; }

		public bool IsEqualHorizontally() { return Left == Right; }
	}
}