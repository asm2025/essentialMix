using System.Collections.Generic;

namespace essentialMix.Collections;

public interface IReadOnlyBoundList<T> : IReadOnlyList<T>
{
	int Capacity { get; }
	int Limit { get; }
}