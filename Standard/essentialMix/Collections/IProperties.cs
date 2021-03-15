using System;

namespace essentialMix.Collections
{
	public interface IProperties : IReadOnlyKeyedCollection<string, IProperty>, IComparable<IProperties>, IComparable, IEquatable<IProperties>, ICloneable
	{
		void Reset();
	}
}