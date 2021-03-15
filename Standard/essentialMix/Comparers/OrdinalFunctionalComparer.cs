using System;

namespace essentialMix.Comparers
{
	[Serializable]
	public sealed class OrdinalFunctionalComparer : StringFunctionalComparer
	{
		private readonly StringComparison _comparison;
		private readonly FunctionalComparerMethod _method;
		private readonly StringComparer _hashCodeComparer;

		public OrdinalFunctionalComparer(bool ignoreCase, FunctionalComparerMethod method)
		{
			if (ignoreCase)
			{
				_comparison = StringComparison.OrdinalIgnoreCase;
				_hashCodeComparer = OrdinalIgnoreCase;
			}
			else
			{
				_comparison = StringComparison.Ordinal;
				_hashCodeComparer = Ordinal;
			}

			_method = method;
		}

		public override int Compare(string x, string y)
		{
			if (x == y) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			if (x.Length < y.Length) return -1;

			switch (_method)
			{
				case FunctionalComparerMethod.Contains:
					return x.IndexOf(y, _comparison) > -1 ? 0 : -1;
				case FunctionalComparerMethod.EndsWith:
					return _comparison == StringComparison.OrdinalIgnoreCase
						? string.Compare(x, x.Length - y.Length, y, 0, y.Length, _comparison)
						: string.CompareOrdinal(x, x.Length - y.Length, y, 0, y.Length);
				default:
					return _comparison == StringComparison.OrdinalIgnoreCase
						? string.Compare(x, 0, y, 0, y.Length, _comparison)
						: string.CompareOrdinal(x, 0, y, 0, y.Length);
			}
		}

		public override bool Equals(string x, string y)
		{
			if (x == y) return true;
			if (x == null || y == null || x.Length < y.Length) return false;

			switch (_method)
			{
				case FunctionalComparerMethod.Contains:
					return x.IndexOf(y, _comparison) > -1;
				case FunctionalComparerMethod.EndsWith:
					return (_comparison == StringComparison.OrdinalIgnoreCase
						? string.Compare(x, x.Length - y.Length, y, 0, y.Length, _comparison)
						: string.CompareOrdinal(x, x.Length - y.Length, y, 0, y.Length)) == 0;
				default:
					return (_comparison == StringComparison.OrdinalIgnoreCase
						? string.Compare(x, 0, y, 0, y.Length, _comparison)
						: string.CompareOrdinal(x, 0, y, 0, y.Length)) == 0;
			}
		}

		public override int GetHashCode(string obj) { return _hashCodeComparer.GetHashCode(obj); }
	}
}