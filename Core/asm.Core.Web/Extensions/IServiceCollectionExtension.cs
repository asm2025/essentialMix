using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace asm.Core.Web.Extensions
{
	public static class IServiceCollectionExtension
	{
		[NotNull]
		public static IServiceCollection AddDefaultCors([NotNull] this IServiceCollection thisValue, params string[] origins)
		{
			thisValue.AddCors(options => options.AddDefaultCors(origins));
			return thisValue;
		}

		[NotNull]
		public static IServiceCollection AddForwardedHeaders([NotNull] this IServiceCollection thisValue, params string[] allowedHosts)
		{
			return AddForwardedHeaders(thisValue, null, allowedHosts);
		}

		[NotNull]
		public static IServiceCollection AddForwardedHeaders([NotNull] this IServiceCollection thisValue, Action<ForwardedHeadersOptions> configure, params string[] allowedHosts)
		{
			thisValue.Configure<ForwardedHeadersOptions>(options =>
			{
				configure?.Invoke(options);
				options.ForwardedHeaders |= ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

				if (options.AllowedHosts == null || options.AllowedHosts.Count == 0 && allowedHosts != null && allowedHosts.Length > 0)
				{
					options.AllowedHosts = allowedHosts;
				}
			});
			return thisValue;
		}
	}
}