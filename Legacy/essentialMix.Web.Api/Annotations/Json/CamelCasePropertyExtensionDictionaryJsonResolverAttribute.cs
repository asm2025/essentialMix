using essentialMix.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class CamelCasePropertyExtensionDictionaryJsonResolverAttribute : PropertyExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public CamelCasePropertyExtensionDictionaryJsonResolverAttribute()
			: base(new CamelCasePropertyNamesContractResolver
					{
						NamingStrategy = new CamelCasePropertyExtensionDictionaryNamingStrategy()
					})
		{
		}
	}
}