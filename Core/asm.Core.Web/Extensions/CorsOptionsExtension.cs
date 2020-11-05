using System;
using System.Linq;
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
			thisValue.AddDefaultPolicy(builder =>
			{
				builder.AllowAnyMethod()
						.AllowAnyHeader();
				origins = origins?.SkipNullOrEmptyTrim()
								.Distinct(StringComparer.OrdinalIgnoreCase)
								.ToArray();

				if (origins != null && origins.Length > 0 && origins.All(e => e != "*"))
				{
					builder.WithOrigins(origins);
					builder.AllowCredentials();
				}
				else
				{
					builder.AllowAnyOrigin();
					builder.DisallowCredentials();
				}
			});
			return thisValue;
		}
	}
}