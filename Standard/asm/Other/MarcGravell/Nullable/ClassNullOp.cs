namespace asm.Other.MarcGravell.Nullable
{
	public sealed class ClassNullOp<T> : INullOp<T>
		where T : class
	{
		public bool HasValue(T value) { return value != null; }

		public bool AddIfNotNull(ref T accumulator, T value)
		{
			if (value == null) return false;
			accumulator = accumulator == null
				? value
				: Operator<T>.Add(accumulator, value);
			return true;
		}
	}
}