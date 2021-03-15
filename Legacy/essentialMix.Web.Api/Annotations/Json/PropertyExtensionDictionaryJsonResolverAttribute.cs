using JetBrains.Annotations;
using essentialMix.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class PropertyExtensionDictionaryJsonResolverAttribute : CustomJsonResolverAttribute
	{
		/// <inheritdoc />
		public PropertyExtensionDictionaryJsonResolverAttribute()
			: this(new PropertyExtensionDictionaryNamingStrategy())
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionDictionaryJsonResolverAttribute(NamingStrategy nameStrategy)
			: base(nameStrategy)
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionDictionaryJsonResolverAttribute([NotNull] IContractResolver resolver)
			: base(resolver)
		{
		}
	}
}