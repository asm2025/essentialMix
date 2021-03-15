using System;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	public abstract class StringLister<T> : Lister<string> 
		where T : IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		protected StringLister(string targetString, [NotNull] T delimiter)
		{
			Type type = typeof(T);
			if (!type.Is(typeof(char)) && !type.Is(typeof(string))) throw new InvalidOperationException("Generic type must be either char or string types.");
			TargetString = targetString;
			Delimiter = delimiter;
			IsIterable = !string.IsNullOrEmpty(TargetString);
		}

		public override void Reset()
		{
			base.Reset();
			SearchIndex = NextIndex = SearchLength = -1;
		}

		[NotNull]
		public T Delimiter { get; }

		protected int SearchIndex { get; set; } = -1;
		protected int NextIndex { get; set; } = -1;
		protected int SearchLength { get; set; } = -1;
		protected string TargetString { get; }

		protected override int MoveTo(int index)
		{
			if (!IsIterable || Done) return 0;

			int n = 0;

			while (!Done && List.Count <= index)
			{
				if (!FindNext())
				{
					Done = true;
					return n;
				}

				List.Add(TargetString.Substring(SearchIndex, SearchLength));

				if (Index < 0) Index = 0;
				else Index++;

				n++;
			}

			return n;
		}

		protected abstract bool FindNext();
	}

	public class StringLister : StringLister<string>
	{
		public StringLister(string targetString, [NotNull] string delimiter)
			: this(targetString, delimiter, StringComparison.OrdinalIgnoreCase)
		{
		}

		public StringLister(string targetString, [NotNull] string delimiter, StringComparison comparison)
			: base(targetString, delimiter)
		{
			Comparison = comparison;
			if (string.IsNullOrEmpty(delimiter)) throw new ArgumentNullException(nameof(delimiter));
		}

		public StringComparison Comparison { get; }

		protected override bool FindNext()
		{
			if (!IsIterable || Done) return false;

			if (SearchIndex < 0)
			{
				SearchIndex = TargetString.IndexOf(Delimiter, 0, Comparison);

				if (SearchIndex < 0)
				{
					SearchIndex = 0;
					SearchLength = TargetString.Length;
					Done = true;
					return true;
				}
			}
			else
			{
				SearchIndex = NextIndex;
			}

			SearchIndex += Delimiter.Length;

			if (SearchIndex >= TargetString.Length)
			{
				Reset();
				Done = true;
				return false;
			}

			NextIndex = TargetString.IndexOf(Delimiter, SearchIndex, Comparison);

			while (NextIndex > -1 && SearchIndex + 1 == NextIndex)
			{
				SearchIndex = NextIndex + 1;

				if (SearchIndex >= TargetString.Length)
				{
					Reset();
					Done = true;
					return false;
				}

				NextIndex = TargetString.IndexOf(Delimiter, SearchIndex, Comparison);
			}

			SearchLength = NextIndex > -1 ? NextIndex - SearchIndex : TargetString.Length - SearchIndex;
			Done = NextIndex < 0;
			return true;
		}
	}
}