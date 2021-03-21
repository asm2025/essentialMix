using essentialMix.Serialization.Json;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class SnakeCasePropertyNoExtensionDictionaryJsonResolverAttribute : PropertyNoExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public SnakeCasePropertyNoExtensionDictionaryJsonResolverAttribute()
			: base(new SnakeCasePropertyNoExtensionDictionaryNamingStrategy())
		{
		}
	}
}