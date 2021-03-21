using essentialMix.Newtonsoft.Serialization;

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