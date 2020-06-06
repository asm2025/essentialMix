using System;

namespace asm.Collections
{
	public interface IProperties : IReadOnlyKeyedCollection<string, IProperty>, IComparable<IProperties>, IComparable, IEquatable<IProperties>, ICloneable
	{
		void Reset();
	}
}