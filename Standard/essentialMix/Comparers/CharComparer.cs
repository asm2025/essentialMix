using System.Globalization;
using JetBrains.Annotations;

namespace essentialMix.Comparers
{
	public sealed class CharComparer : IGenericComparer<char>
	{
		[NotNull]
		private readonly TextInfo _textInfo;
		private readonly bool _ignoreCase;

		public CharComparer()
			: this(CultureInfo.InvariantCulture, false)
		{
		}

		public CharComparer([NotNull] CultureInfo culture)
			: this(culture, false)
		{
		}

		public CharComparer([NotNull] CultureInfo culture, bool ignoreCase)
		{
			_textInfo = culture.TextInfo;
			_ignoreCase = ignoreCase;
		}

		public int Compare(char x, char y)
		{
			return _ignoreCase
						? _textInfo.ToLower(x).CompareTo(_textInfo.ToLower(y))
						: x.CompareTo(y);
		}

		public bool Equals(char x, char y)
		{
			return Compare(x, y) == 0;
		}

		public int GetHashCode(char obj)
		{
			return _ignoreCase
						? _textInfo.ToLower(obj).GetHashCode()
						: obj.GetHashCode();
		}

		public int Compare(object x, object y)
		{
			return x is not char xc
						? y is char
							? -1
							: 0
						: y is not char yc
							? 1
							: Compare(xc, yc);
		}

		public new bool Equals(object x, object y) { return Compare(x, y) == 0; }

		public int GetHashCode(object obj)
		{
			return obj is not char c
						? ReferenceComparer.Default.GetHashCode(obj)
						: GetHashCode(c);
		}

		public static CharComparer CurrentCulture { get; } = new CharComparer(CultureInfo.CurrentCulture, false);
		public static CharComparer CurrentCultureIgnoreCase { get; } = new CharComparer(CultureInfo.CurrentCulture, true);
		public static CharComparer InvariantCulture { get; } = new CharComparer(CultureInfo.InvariantCulture, false);
		public static CharComparer InvariantCultureIgnoreCase { get; } = new CharComparer(CultureInfo.InvariantCulture, true);
	}
}