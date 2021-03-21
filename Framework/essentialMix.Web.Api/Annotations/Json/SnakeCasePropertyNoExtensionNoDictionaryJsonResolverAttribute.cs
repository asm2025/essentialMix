using essentialMix.Newtonsoft.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
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