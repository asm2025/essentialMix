using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cors.Infrastructure;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class CorsOptionsExtension
	{
		[NotNull]
		public static CorsOptions AddDefaultPolicy([NotNull] this CorsOptions thisValue, params string[] origins)
		{
			thisValue.AddDefaultPolicy(builder => builder.BuildDefaultPolicy(origins));
			return thisValue;
		}

		[NotNull]
		public static CorsOptions AddDefaultPolicy([NotNull] this CorsOptions thisValue, [NotNull] Action<CorsPolicyBuilder> configurePolicy, params string[] origins)
		{
			thisValue.AddDefaultPolicy(builder =>
			{
				builder.BuildDefaultPolicy(origins);
				configurePolicy(builder);
			});
			return thisValue;
		}
	}
}