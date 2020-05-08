using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace asm.Core.Web.Extensions
{
	public static class IMvcCoreBuilderExtension
	{
		[NotNull]
		public static IMvcCoreBuilder AddDefaultCors([NotNull] this IMvcCoreBuilder thisValue, params string[] origins)
		{
			thisValue.AddCors(options => options.AddDefaultCors(origins));
			return thisValue;
		}
	}
}