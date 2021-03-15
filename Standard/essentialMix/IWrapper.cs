using JetBrains.Annotations;

namespace essentialMix
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