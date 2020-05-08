using asm.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace asm.Web.Api.Annotations.Json
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