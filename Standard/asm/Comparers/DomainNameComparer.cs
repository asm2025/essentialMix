using System.Collections.Generic;

namespace asm.Comparers
{
	// todo complete DomainNameComparer
	public sealed class DomainNameComparer : GenericComparer<string>
	{
		public new static DomainNameComparer Default { get; } = new DomainNameComparer();

		/// <inheritdoc />
		public DomainNameComparer() 
		{
		}

		/// <inheritdoc />
		public DomainNameComparer(IComparer<string> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public DomainNameComparer(IComparer<string> comparer, IEqualityComparer<string> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(string x, string y) { return 0; }

		public override bool Equals(string x, string y) { return Compare(x, y) == 0; }
	}
}