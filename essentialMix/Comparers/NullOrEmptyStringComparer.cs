using System;

namespace essentialMix.Comparers;

[Serializable]
public class NullOrEmptyStringComparer : StringComparer, IGenericComparer<string>
{
	public static NullOrEmptyStringComparer Default { get; } = new NullOrEmptyStringComparer();

	/// <inheritdoc />
	public NullOrEmptyStringComparer() 
	{
	}

	/// <inheritdoc />
	public override int Compare(string x, string y)
	{
		if (!string.IsNullOrEmpty(x)) return -1;
		return string.IsNullOrEmpty(y)
					? 0
					: 1;
	}

	/// <inheritdoc />
	public override bool Equals(string x, string y) { return OrdinalIgnoreCase.Equals(x, y); }

	/// <inheritdoc />
	public override int GetHashCode(string obj) { return OrdinalIgnoreCase.GetHashCode(obj); }
}