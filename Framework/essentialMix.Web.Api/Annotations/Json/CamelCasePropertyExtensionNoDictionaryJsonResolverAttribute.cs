using essentialMix.Newtonsoft.Serialization;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class CamelCasePropertyExtensionNoDictionaryJsonResolverAttribute : PropertyExtensionNoDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public CamelCasePropertyExtensionNoDictionaryJsonResolverAttribute()
			: base(new CamelCasePropertyNamesContractResolver
					{
						NamingStrategy = new CamelCasePropertyExtensionNoDictionaryNamingStrategy()
					})
		{
		}
	}
}