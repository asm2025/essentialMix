using essentialMix.Newtonsoft.Serialization;

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