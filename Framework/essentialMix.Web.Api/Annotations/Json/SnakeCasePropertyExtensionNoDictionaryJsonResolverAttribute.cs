using essentialMix.Serialization.Json;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class SnakeCasePropertyExtensionNoDictionaryJsonResolverAttribute : PropertyExtensionNoDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public SnakeCasePropertyExtensionNoDictionaryJsonResolverAttribute()
			: base(new SnakeCasePropertyExtensionNoDictionaryNamingStrategy())
		{
		}
	}
}