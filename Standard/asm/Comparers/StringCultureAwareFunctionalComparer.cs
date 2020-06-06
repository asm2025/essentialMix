using System;
using System.Globalization;
using JetBrains.Annotations;

namespace asm.Comparers
{
	[Serializable]
	public sealed class StringCultureAwareFunctionalComparer : StringFunctionalComparer
	{
		private readonly CompareInfo _compareInfo;
		private readonly CompareOptions _compareOptions;
		private readonly FunctionalComparerMethod _method;

		internal StringCultureAwareFunctionalComparer([NotNull] CultureInfo culture, bool ignoreCase, FunctionalComparerMethod method)
		{
			_compareInfo = culture.CompareInfo;
			_compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
			_method = method;
		}

		internal StringCultureAwareFunctionalComparer(CompareInfo compareInfo, bool ignoreCase, FunctionalComparerMethod method)
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

			return _method switch
			{
				FunctionalComparerMethod.StartsWith => _compareInfo.Compare(x, 0, y.Length, y, 0, y.Length, _compareOptions),
				FunctionalComparerMethod.EndsWith => _compareInfo.Compare(x, x.Length - y.Length, y.Length, y, 0, y.Length, _compareOptions),
				_ => _compareInfo.IndexOf(x, y, _compareOptions) > -1
						? 0
						: -1
			};
		}

		public override bool Equals(string x, string y)
		{
			if (x == y) return true;
			if (x == null || y == null || x.Length < y.Length) return false;

			return _method switch
			{
				FunctionalComparerMethod.StartsWith => _compareInfo.Compare(x, 0, y.Length, y, 0, y.Length, _compareOptions) == 0,
				FunctionalComparerMethod.EndsWith => _compareInfo.Compare(x, x.Length - y.Length, y.Length, y, 0, y.Length, _compareOptions) == 0,
				_ => _compareInfo.IndexOf(x, y, _compareOptions) > -1,
			};
		}

		public override int GetHashCode(string obj) { return _compareInfo.GetHashCode(obj, _compareOptions); }
	}
}