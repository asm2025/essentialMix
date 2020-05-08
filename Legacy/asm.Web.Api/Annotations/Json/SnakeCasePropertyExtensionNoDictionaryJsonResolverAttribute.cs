using asm.Serialization.Json;

namespace asm.Web.Api.Annotations.Json
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