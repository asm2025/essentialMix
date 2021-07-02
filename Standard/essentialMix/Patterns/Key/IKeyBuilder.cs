using JetBrains.Annotations;

namespace essentialMix.Patterns.Key
{
	/// <summary>
	/// This interface allows custom types to be keys
	/// </summary>
	public interface IKeyBuilder
	{
		void Build([NotNull] KeyBuilder builder);
	}
}