using essentialMix.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class CamelCasePropertyNoExtensionNoDictionaryJsonResolverAttribute : PropertyNoExtensionNoDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public CamelCasePropertyNoExtensionNoDictionaryJsonResolverAttribute()
			: base(new CamelCasePropertyNamesContractResolver
			{
				NamingStrategy = new CamelCasePropertyNoExtensionNoDictionaryNamingStrategy()
			})
		{
		}
	}
}