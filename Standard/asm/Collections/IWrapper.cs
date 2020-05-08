using JetBrains.Annotations;

namespace asm.Collections
{
	public interface IWrapper : IWrapper<object>
	{
		[NotNull]
		new object Source { get; }
	}

	public interface IWrapper<out TSource>
	{
		[NotNull]
		TSource Source { get; }
	}
}