using asm.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace asm.Web.Api.Annotations.Json
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