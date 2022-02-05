using System.Collections;
using System.Collections.Generic;

namespace essentialMix.Comparers;

public interface IGenericComparer<in T> : IComparer<T>, IComparer, IEqualityComparer<T>, IEqualityComparer
{
}