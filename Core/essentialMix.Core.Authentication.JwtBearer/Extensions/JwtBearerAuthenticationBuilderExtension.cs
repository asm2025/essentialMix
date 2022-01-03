using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class JwtBearerAuthenticationBuilderExtension
	{
		[NotNull]
		public static AuthenticationBuilder AddJwtBearerOptions([NotNull] this AuthenticationBuilder thisValue, [NotNull] Action<JwtBearerOptions> configureOptions)
		{
			return AddJwtBearerOptions(thisValue, true, configureOptions);
		}

		[NotNull]
		public static AuthenticationBuilder AddJwtBearerOptions([NotNull] this AuthenticationBuilder thisValue, bool useAsDefault, [NotNull] Action<JwtBearerOptions> configureOptions)
		{
			return useAsDefault
				? thisValue.AddJwtBearer(configureOptions)
				: thisValue.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions);
		}
	}
}