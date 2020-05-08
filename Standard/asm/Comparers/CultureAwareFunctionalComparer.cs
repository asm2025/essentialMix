using System;
using System.Globalization;
using JetBrains.Annotations;

namespace asm.Comparers
{
	[Serializable]
	public sealed class CultureAwareFunctionalComparer : StringFunctionalComparer
	{
		private readonly CompareInfo _compareInfo;
		private readonly CompareOptions _compareOptions;
		private readonly FunctionalComparerMethod _method;

		internal CultureAwareFunctionalComparer([NotNull] CultureInfo culture, bool ignoreCase, FunctionalComparerMethod method)
		{
			_compareInfo = culture.CompareInfo;
			_compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
			_method = method;
		}

		internal CultureAwareFunctionalComparer(CompareInfo compareInfo, bool ignoreCase, FunctionalComparerMethod method)
		{
			_compareInfo = compareInfo;
			_compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
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
					return _compareInfo.IndexOf(x, y, _compareOptions) > -1 ? 0 : -1;
				case FunctionalComparerMethod.EndsWith:
					return _compareInfo.Compare(x, x.Length - y.Length, y.Length, y, 0, y.Length, _compareOptions);
				default:
					return _compareInfo.Compare(x, 0, y.Length, y, 0, y.Length, _compareOptions);
			}
		}

		public override bool Equals(string x, string y)
		{
			if (x == y) return true;
			if (x == null || y == null || x.Length < y.Length) return false;

			switch (_method)
			{
				case FunctionalComparerMethod.Contains:
					return _compareInfo.IndexOf(x, y, _compareOptions) > -1;
				case FunctionalComparerMethod.EndsWith:
					return _compareInfo.Compare(x, x.Length - y.Length, y.Length, y, 0, y.Length, _compareOptions) == 0;
				default:
					return _compareInfo.Compare(x, 0, y.Length, y, 0, y.Length, _compareOptions) == 0;
			}
		}

		public override int GetHashCode(string obj) { return _compareInfo.GetHashCode(obj, _compareOptions); }
	}
}