using asm.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace asm.Web.Api.Annotations.Json
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