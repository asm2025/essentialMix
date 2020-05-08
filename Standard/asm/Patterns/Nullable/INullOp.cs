namespace asm.Patterns.Nullable
{
	public interface INullOp<T>
	{
		bool HasValue(T value);

		bool AddIfNotNull(ref T accumulator, T value);
	}
}
