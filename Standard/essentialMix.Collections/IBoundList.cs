using System.Collections.Generic;

namespace essentialMix.Collections;

public interface IBoundList<T> : IList<T>
{
	int Capacity { get; set; }
	int Limit { get; set; }
}