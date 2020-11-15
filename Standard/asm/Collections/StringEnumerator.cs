using System;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{
	public abstract class StringEnumerator<T> : Enumerator<string> 
		where T : IComparable<T>, IComparable, IEquatable<T>, IConvertible
	{
		protected StringEnumerator(string targetString, [NotNull] T delimiter)
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
			NextIndex = SearchLength = -1;
		}

		[NotNull]
		public override string Current
		{
			get
			{
				if (Index < 0 || SearchLength <= 0) throw new InvalidOperationException("No value available.");
				return TargetString.Substring(Index, SearchLength);
			}
		}

		[NotNull]
		public T Delimiter { get; }
		protected int NextIndex { get; set; } = -1;
		protected int SearchLength { get; set; } = -1;
		protected string TargetString { get; }

		public override bool MoveNext() { return FindNext(); }

		protected abstract bool FindNext();
	}

	public class StringEnumerator : StringEnumerator<string>
	{
		public StringEnumerator(string targetString, [NotNull] string delimiter)
			: this(targetString, delimiter, StringComparison.OrdinalIgnoreCase)
		{
		}

		public StringEnumerator(string targetString, [NotNull] string delimiter, StringComparison comparison)
			: base(targetString, delimiter)
		{
			Comparison = comparison;
			if (string.IsNullOrEmpty(delimiter)) throw new ArgumentNullException(nameof(delimiter));
		}

		public StringComparison Comparison { get; }

		protected override bool FindNext()
		{
			if (!IsIterable || Done) return false;

			if (Index < 0)
			{
				Index = 0;
				NextIndex = TargetString.IndexOf(Delimiter, Index + 1, Comparison);
				SearchLength = NextIndex > -1
									? NextIndex
									: TargetString.Length;
				return true;
			}
			
			if (NextIndex < 0 || NextIndex >= TargetString.Length - 1)
			{
				Reset();
				Done = true;
				return false;
			}

			Index += Delimiter.Length + 1;
			NextIndex = TargetString.IndexOf(Delimiter, Index + 1, Comparison);

			while (NextIndex > -1 && Index + Delimiter.Length == NextIndex)
			{
				Index = NextIndex + Delimiter.Length;

				if (Index >= TargetString.Length - 1)
				{
					Reset();
					Done = true;
					return false;
				}

				NextIndex = TargetString.IndexOf(Delimiter, Index + 1, Comparison);
			}

			SearchLength = NextIndex > -1
								? NextIndex - Index
								: TargetString.Length - Index;
			Done = NextIndex < 0;
			return true;
		}
	}
}