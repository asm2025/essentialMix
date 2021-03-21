using JetBrains.Annotations;
using essentialMix.Newtonsoft.Serialization;
using Newtonsoft.Json.Serialization;

namespace essentialMix.Web.Api.Annotations.Json
{
	public class PropertyNoExtensionNoDictionaryJsonResolverAttribute : PropertyExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public PropertyNoExtensionNoDictionaryJsonResolverAttribute()
			: this(new PropertyNoExtensionNoDictionaryNamingStrategy())
		{
		}

		/// <inheritdoc />
		protected PropertyNoExtensionNoDictionaryJsonResolverAttribute(NamingStrategy nameStrategy)
			: base(nameStrategy)
		{
		}

		/// <inheritdoc />
		protected PropertyNoExtensionNoDictionaryJsonResolverAttribute([NotNull] IContractResolver resolver)
			: base(resolver)
		{
		}
	}
}