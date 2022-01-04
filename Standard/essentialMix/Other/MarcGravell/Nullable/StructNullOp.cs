// ReSharper disable once CheckNamespace
namespace Other.MarcGravell.Nullable;

public sealed class StructNullOp<T> : INullOp<T>, INullOp<T?>
	where T : struct
{
	public bool HasValue(T value) { return true; }

	public bool AddIfNotNull(ref T accumulator, T value)
	{
		accumulator = Operator<T>.Add(accumulator, value);
		return true;
	}

	public bool HasValue(T? value) { return value.HasValue; }

	public bool AddIfNotNull(ref T? accumulator, T? value)
	{
		if (!value.HasValue) return false;
		accumulator = accumulator.HasValue
						? Operator<T>.Add(
										accumulator.GetValueOrDefault(),
										value.GetValueOrDefault())
						: value;
		return true;
	}
}