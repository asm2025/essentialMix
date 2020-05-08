using asm.Serialization.Json;

namespace asm.Web.Api.Annotations.Json
{
	public class SnakeCasePropertyNoExtensionNoDictionaryJsonResolverAttribute : PropertyNoExtensionNoDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public SnakeCasePropertyNoExtensionNoDictionaryJsonResolverAttribute()
			: base(new SnakeCasePropertyNoExtensionNoDictionaryNamingStrategy())
		{
		}
	}
}