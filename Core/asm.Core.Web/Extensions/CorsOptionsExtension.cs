using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cors.Infrastructure;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class CorsOptionsExtension
	{
		[NotNull]
		public static CorsOptions AddDefaultCors([NotNull] this CorsOptions thisValue, params string[] origins)
		{
			thisValue.AddDefaultPolicy(builder => builder.BuildDefaultCors(origins));
			return thisValue;
		}

		[NotNull]
		public static CorsOptions AddDefaultCors([NotNull] this CorsOptions thisValue, [NotNull] Action<CorsPolicyBuilder> configurePolicy, params string[] origins)
		{
			thisValue.AddDefaultPolicy(builder =>
			{
				builder.BuildDefaultCors(origins);
				configurePolicy(builder);
			});
			return thisValue;
		}
	}
}