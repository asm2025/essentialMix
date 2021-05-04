using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Cors.Infrastructure;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class CorsPolicyBuilderExtension
	{
		[NotNull]
		public static CorsPolicyBuilder BuildDefaultPolicy([NotNull] this CorsPolicyBuilder thisValue, params string[] origins)
		{
			thisValue.AllowAnyMethod()
					.AllowAnyHeader();
			origins = origins?.SkipNullOrEmptyTrim()
							.Distinct(StringComparer.OrdinalIgnoreCase)
							.ToArray();

			if (origins is { Length: > 0 } && origins.All(e => e != "*"))
			{
				thisValue.WithOrigins(origins);
				thisValue.AllowCredentials();
			}
			else
			{
				thisValue.AllowAnyOrigin();
				thisValue.DisallowCredentials();
			}

			return thisValue;
		}
	}
}