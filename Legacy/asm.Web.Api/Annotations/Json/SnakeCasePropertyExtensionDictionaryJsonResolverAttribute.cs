using asm.Serialization.Json;

namespace asm.Web.Api.Annotations.Json
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