using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Web.Drawing
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[TypeConverter(typeof(PxUnitConverter))]
	public struct PxUnit : IComparable, IComparable<PxUnit>
	{
		internal const int MIN_VALUE = -32768;
		internal const int MAX_VALUE = 0x7fff;

		private readonly PxUnitType _type;

		/// <exception cref="ArgumentOutOfRangeException">
		/// 	<c>value</c>
		/// 	is out of range.</exception>
		public PxUnit(int value)
		{
			if (!value.InRange(MIN_VALUE, MAX_VALUE)) throw new ArgumentOutOfRangeException(nameof(value));
			Value = value;
			_type = PxUnitType.Pixel;
		}

		/// <exception cref="ArgumentOutOfRangeException">
		/// 	<c>value</c>
		/// 	is out of range.</exception>
		public PxUnit(double value)
		{
			if (!value.InRange(MIN_VALUE, MAX_VALUE)) throw new ArgumentOutOfRangeException(nameof(value));
			Value = (int)value;
			_type = PxUnitType.Pixel;
		}

		/// <exception cref="ArgumentOutOfRangeException">
		/// 	<c>value</c>
		/// 	is out of range.</exception>
		public PxUnit(double value, PxUnitType type)
		{
			if (!value.InRange(MIN_VALUE, MAX_VALUE)) throw new ArgumentOutOfRangeException(nameof(value));
			Value = type == PxUnitType.Pixel ? Convert.ToInt32(value) : value;
			_type = type;
		}

		public PxUnit(Unit unit)
		{
			switch (unit.Type)
			{
				case UnitType.Percentage:
					Value = unit.Value;
					_type = PxUnitType.Percentage;
					break;
				default:
					Value = Convert.ToInt32(unit.Value);
					_type = PxUnitType.Pixel;
					break;
			}
		}

		public PxUnit(string value)
			: this(value, CultureInfo.CurrentUICulture, PxUnitType.Pixel) { }

		public PxUnit(string value, CultureInfo culture)
			: this(value, culture, PxUnitType.Pixel) { }

		/// <exception cref="FormatException"></exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// 	<c>value</c>
		/// 	is out of range.</exception>
		internal PxUnit(string value, CultureInfo culture, PxUnitType defaultType)
		{
			if (string.IsNullOrEmpty(value))
			{
				Value = 0.0D;
				_type = 0;
			}
			else
			{
				if (culture == null) culture = CultureInfo.CurrentUICulture;

				string str = value.Trim().ToLower(culture);
				int length = str.Length;
				int num2 = -1;

				for (int i = 0; i < length; i++)
				{
					char ch = str[i];

					if ((ch < '0' || ch > '9') && ch != '-' && ch != '.' && ch != ',') break;
					num2 = i;
				}

				if (num2 == -1) throw new FormatException($"Unit string '{value}' has no digits");

				_type = num2 < length - 1 ? GetTypeFromString(str.Substring(num2 + 1).Trim()) : defaultType;
				string text = str.Substring(0, num2 + 1);
				float v = Convert.ToSingle(text);
				if (!v.InRange(MIN_VALUE, MAX_VALUE)) throw new ArgumentOutOfRangeException(nameof(value));
				Value = _type == PxUnitType.Pixel
							? (int)v
							: v;
			}
		}

		public bool IsEmpty => _type == 0;

		public PxUnitType Type => !IsEmpty ? _type : PxUnitType.Pixel;

		public double Value { get; }

		public double ToDouble => CanReturnValue() ? Value : 0.0D;

		public float ToFloat => CanReturnValue() ? Convert.ToSingle(Value) : 0.0f;

		public int ToInt => CanReturnValue() ? Convert.ToInt32(Value) : 0;

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 397;
				hash = (hash * 397) ^ Type.GetHashCode();
				hash = (hash * 397) ^ Value.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case null:
					return false;
				case PxUnit pxUnit:
					return pxUnit.Type == _type && pxUnit.Value.Equals(Value);
				case Unit unit:
					if (unit.Type == UnitType.Percentage) return _type == PxUnitType.Percentage && unit.Value.Equals(Value);
					return Value.Equals(unit.Value);
			}

			return false;
		}

		public override string ToString() { return ToString((IFormatProvider)CultureInfo.CurrentUICulture); }

		[NotNull]
		public string ToString(CultureInfo culture) { return ToString((IFormatProvider)culture); }

		[NotNull]
		public string ToString(IFormatProvider formatProvider)
		{
			return IsEmpty
						? string.Empty
						: $"{(_type == PxUnitType.Pixel ? Convert.ToInt32(Value).ToString(formatProvider) : Convert.ToSingle(Value).ToString(formatProvider))}{GetStringFromType(_type)}";
		}

		public int CompareTo(object obj)
		{
			switch (obj)
			{
				case PxUnit pxUnit:
					return CompareTo(pxUnit);
				case Unit unit:
					return CompareTo(unit);
			}
			return -1;
		}

		public int CompareTo(Unit other) { return CompareTo((PxUnit)other); }

		public int CompareTo(PxUnit other)
		{
			int n = _type.CompareTo(other.Type);
			return n != 0 ? n : Value.CompareTo(other.Value);
		}

		public int CompareTo(ValueType value) { return Value.CompareTo(Convert.ToDouble(value)); }

		public Unit ToUnit() { return ToUnit(this); }

		private bool CanReturnValue() { return Convert.ToBoolean(!IsEmpty && _type == PxUnitType.Pixel); }

		public static bool operator ==(PxUnit left, PxUnit right) { return left.Type == right.Type && left.Value.Equals(right.Value); }

		public static bool operator !=(PxUnit left, PxUnit right)
		{
			if (left.Type == right.Type) return !left.Value.Equals(right.Value);
			return true;
		}

		public static bool operator ==(PxUnit left, Unit right)
		{
			if (right.Type == UnitType.Percentage) return left.Type == PxUnitType.Percentage && left.Value.Equals(right.Value);
			return left.Value.Equals(right.Value);
		}

		public static bool operator !=(PxUnit left, Unit right)
		{
			if (right.Type == UnitType.Percentage) return left.Type != PxUnitType.Percentage || !left.Value.Equals(right.Value);
			return !left.Value.Equals(right.Value);
		}

		public static bool operator ==(PxUnit left, ValueType right) { return left.Value.Equals(Convert.ToDouble(right)); }

		public static bool operator !=(PxUnit left, ValueType right) { return !left.Value.Equals(Convert.ToDouble(right)); }

		public static implicit operator PxUnit(int n) { return Pixel(n); }

		public static implicit operator PxUnit(Unit unit) { return FromUnit(unit); }

		public static PxUnit FromUnit(Unit unit)
		{
			if (unit.IsEmpty) return new PxUnit();

			switch (unit.Type)
			{
				case UnitType.Percentage:
					return new PxUnit(unit.Value, PxUnitType.Percentage);
				default:
					return new PxUnit(unit.Value, PxUnitType.Pixel);
			}
		}

		public static Unit ToUnit(PxUnit unit)
		{
			if (unit.IsEmpty) return Unit.Empty;

			switch (unit.Type)
			{
				case PxUnitType.Percentage:
					return Unit.Percentage(unit.Value);
				default:
					return Unit.Pixel(Convert.ToInt32(unit.Value));
			}
		}

		public static PxUnit Parse(string s) { return new PxUnit(s, CultureInfo.CurrentUICulture); }

		public static PxUnit Parse(string s, CultureInfo culture) { return new PxUnit(s, culture); }

		public static PxUnit Percentage(double n) { return new PxUnit(n, PxUnitType.Percentage); }

		public static PxUnit Pixel(int n) { return new PxUnit(n); }

		public static bool IsTypeSupported(Unit unit) { return IsTypeSupported(unit.Type); }

		public static bool IsTypeSupported(UnitType type)
		{
			bool b;

			switch (type)
			{
				case UnitType.Pixel:
				case UnitType.Percentage:
					b = true;
					break;
				default:
					b = false;
					break;
			}

			return b;
		}

		[NotNull]
		private static string GetStringFromType(PxUnitType type)
		{
			switch (type)
			{
				case PxUnitType.Pixel:
					return "px";
				case PxUnitType.Percentage:
					return "%";
			}

			return string.Empty;
		}

		/// <exception cref="ArgumentOutOfRangeException">
		/// 	<c>value</c>
		/// 	is out of range.</exception>
		private static PxUnitType GetTypeFromString(string value)
		{
			if (string.IsNullOrEmpty(value)) return PxUnitType.Pixel;
			if (value.Equals("%")) return PxUnitType.Percentage;
			if (!value.Equals("px", StringComparison.OrdinalIgnoreCase)) throw new ArgumentOutOfRangeException(nameof(value));
			return PxUnitType.Pixel;
		}
	}
}
