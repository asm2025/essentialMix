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
				
				if (origins != null && origins.Length > 0)
				{
					builder.WithOrigins(origins);
					if (origins.All(e => e == "*")) builder.AllowCredentials();
				}
				else
				{
					builder.AllowAnyOrigin();
				}
			});
			return thisValue;
		}
	}
}