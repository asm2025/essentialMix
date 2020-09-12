using JetBrains.Annotations;

namespace asm
{
	public interface IWrapper<out TSource>
	{
		[NotNull]
		TSource Source { get; }
	}

	public interface IWrapper : IWrapper<object>
	{
	}
}