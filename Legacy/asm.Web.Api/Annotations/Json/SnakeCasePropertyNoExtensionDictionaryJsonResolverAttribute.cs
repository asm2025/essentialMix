using asm.Serialization.Json;

namespace asm.Web.Api.Annotations.Json
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