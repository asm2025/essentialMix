using essentialMix.Serialization.Json;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class SnakeCasePropertyExtensionDictionaryJsonResolverAttribute : PropertyExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public SnakeCasePropertyExtensionDictionaryJsonResolverAttribute()
			: base(new SnakeCasePropertyExtensionDictionaryNamingStrategy())
		{
		}
	}
}