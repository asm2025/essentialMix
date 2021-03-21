using essentialMix.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class CamelCasePropertyNoExtensionDictionaryJsonResolverAttribute : PropertyNoExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public CamelCasePropertyNoExtensionDictionaryJsonResolverAttribute()
				: base(new CamelCasePropertyNamesContractResolver
				{
					NamingStrategy = new CamelCasePropertyNoExtensionDictionaryNamingStrategy()
				})
		{
		}
	}
}