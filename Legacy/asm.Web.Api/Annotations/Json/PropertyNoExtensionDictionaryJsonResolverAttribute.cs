using JetBrains.Annotations;
using asm.Serialization.Json;
using Newtonsoft.Json.Serialization;

namespace asm.Web.Api.Annotations.Json
{
	public class PropertyNoExtensionDictionaryJsonResolverAttribute : PropertyExtensionDictionaryJsonResolverAttribute
	{
		/// <inheritdoc />
		public PropertyNoExtensionDictionaryJsonResolverAttribute()
			: this(new PropertyNoExtensionDictionaryNamingStrategy())
		{
		}

		/// <inheritdoc />
		protected PropertyNoExtensionDictionaryJsonResolverAttribute(NamingStrategy nameStrategy)
			: base(nameStrategy)
		{
		}

		/// <inheritdoc />
		protected PropertyNoExtensionDictionaryJsonResolverAttribute([NotNull] IContractResolver resolver)
			: base(resolver)
		{
		}
	}
}