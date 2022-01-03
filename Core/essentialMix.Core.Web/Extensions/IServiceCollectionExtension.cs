using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class IServiceCollectionExtension
	{
		[NotNull]
		public static IServiceCollection AddDefaultCorsPolicy([NotNull] this IServiceCollection thisValue, params string[] origins)
		{
			thisValue.AddCors(options => options.AddDefaultPolicy(origins));
			return thisValue;
		}

		[NotNull]
		public static IServiceCollection AddDefaultCorsPolicy([NotNull] this IServiceCollection thisValue, [NotNull] Action<CorsPolicyBuilder> configurePolicy, params string[] origins)
		{
			thisValue.AddCors(options => options.AddDefaultPolicy(configurePolicy, origins));
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

				if (options.AllowedHosts.Count == 0 && allowedHosts is { Length: > 0 })
				{
					options.AllowedHosts = allowedHosts;
				}
			});
			return thisValue;
		}
	}
}