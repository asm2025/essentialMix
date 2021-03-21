using JetBrains.Annotations;
using essentialMix.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class PropertyExtensionNoDictionaryJsonResolverAttribute : PropertyExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public PropertyExtensionNoDictionaryJsonResolverAttribute()
			: this(new PropertyExtensionNoDictionaryNamingStrategy())
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionNoDictionaryJsonResolverAttribute(NamingStrategy nameStrategy)
			: base(nameStrategy)
		{
		}

		/// <inheritdoc />
		protected PropertyExtensionNoDictionaryJsonResolverAttribute([NotNull] IContractResolver resolver)
			: base(resolver)
		{
		}
	}
}