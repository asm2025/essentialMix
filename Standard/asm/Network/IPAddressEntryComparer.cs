using asm.Extensions;
using asm.Comparers;

namespace asm.Network
{
	public class IPAddressEntryComparer : GenericComparer<IPAddressEntry>
	{
		public new static IPAddressEntryComparer Default { get; } = new IPAddressEntryComparer();

		public IPAddressEntryComparer()
		{
		}

		public override int Compare(IPAddressEntry x, IPAddressEntry y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			if (x.IsEmpty && y.IsEmpty) return 0;
			if (!x.IsEmpty) return -1;
			return !y.IsEmpty ? 1 : x.Value.CompareOrdinal(y.Value);
		}

		public override bool Equals(IPAddressEntry x, IPAddressEntry y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			if (x.IsEmpty && y.IsEmpty) return true;
			if (x.IsEmpty || y.IsEmpty) return false;
			return x.Value.IsSameOrdinal(y.Value);
		}
	}
}