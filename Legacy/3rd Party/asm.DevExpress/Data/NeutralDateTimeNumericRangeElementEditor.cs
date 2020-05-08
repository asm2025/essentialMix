using asm.Helpers;
using DevExpress.Data.Mask;

namespace asm.DevExpress.Data
{
	internal class NeutralDateTimeNumericRangeElementEditor : DateTimeNumericRangeElementEditor
	{
		private const string MINUS_NON_VALUE_DISPLAY_TEXT = "-_";
		private const string NEGATIVE_ZERO_VALUE_DISPLAY_TEXT = "-0";

		private readonly bool _allowNegativeZero;
		private readonly int _defaultValue;
		private bool _isNegativeZero;
		private bool _isMinusNonValue;

		public NeutralDateTimeNumericRangeElementEditor(bool allowNegativeZero, bool isNegativeZero, int initialValue, int defaultValue, int minValue, int maxValue,
			int minDigits, int maxDigits)
			: base(initialValue, minValue, maxValue, minDigits, maxDigits)
		{
			_allowNegativeZero = allowNegativeZero;
			_isNegativeZero = _allowNegativeZero && isNegativeZero;
			_defaultValue = defaultValue;
		}

		public override string DisplayText
		{
			get
			{
				if (_isMinusNonValue) return MINUS_NON_VALUE_DISPLAY_TEXT;
				return _isNegativeZero ? NEGATIVE_ZERO_VALUE_DISPLAY_TEXT : base.DisplayText;
			}
		}

		public override bool Delete()
		{
			if (_isMinusNonValue)
			{
				SetUntouchedValue(_defaultValue);
				_isMinusNonValue = false;
				_isNegativeZero = false;
				return true;
			}

			bool ok = base.Delete();
			SetUntouchedValue(_defaultValue);
			return ok;
		}

		public override bool Insert(string inserted)
		{
			if (MinValue < 0)
			{
				if (inserted == "-")
				{
					_isMinusNonValue = true;
					inserted = "1";
					return SetTouchedNegativeValue(inserted);
				}
				if (_isMinusNonValue)
				{
					_isMinusNonValue = false;
					return SetTouchedNegativeValue(inserted);
				}
				if (digitsEntered > 0 && CurrentValue <= 0)
				{
					if (_isNegativeZero)
					{
						return SetTouchedNegativeValue($"{inserted}");
					}
					return SetTouchedNegativeValue($"{-CurrentValue}{inserted}");
				}
			}

			_isNegativeZero = false;
			return base.Insert(inserted);
		}

		public override bool SpinDown()
		{
			if (!_allowNegativeZero) return base.SpinDown();

			if (!_isNegativeZero && CurrentValue == 0)
			{
				_isNegativeZero = true;
				return false;
			}
			_isNegativeZero = false;
			return base.SpinDown();
		}

		public override bool SpinUp()
		{
			if (!_allowNegativeZero) return base.SpinUp();

			if (CurrentValue == -1)
			{
				SetTouchedNegativeValue("0");
				return true;
			}

			if (_isNegativeZero)
			{
				_isNegativeZero = false;
				return false;
			}
			_isNegativeZero = false;
			return base.SpinUp();
		}

		public override int GetResult()
		{
			if (_isMinusNonValue) return _defaultValue;
			return _isNegativeZero ? TimeSpanHelper.NegativeZeroValue : base.GetResult();
		}

		private bool SetTouchedNegativeValue(string insertedValue)
		{
			int previousDigitsEntered = digitsEntered;
			int previousCurrentValue = CurrentValue;
			SetUntouchedValue(0);
			if (base.Insert(insertedValue))
			{
				_isNegativeZero = _allowNegativeZero && CurrentValue == 0;
				int currentValue = -CurrentValue;
				SetUntouchedValue(currentValue);
				digitsEntered = 1;
				return true;
			}

			SetUntouchedValue(previousCurrentValue);
			digitsEntered = previousDigitsEntered;
			return false;
		}
	}
}