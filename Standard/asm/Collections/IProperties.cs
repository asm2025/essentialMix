using System;

namespace asm.Collections
{
	public interface IProperties : IObservableKeyedCollection<string, IProperty>, IComparable<IProperties>, IComparable, IEquatable<IProperties>, ICloneable
	{
		void Reset();
	}
}