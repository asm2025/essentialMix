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
		public static AuthenticationBuilder AddJwtBearerOptions([NotNull] this AuthenticationBuilder thisValue, Action<JwtBearerOptions> configureOptions = null)
		{
			return AddJwtBearerOptions(thisValue, true, configureOptions);
		}

		public static AuthenticationBuilder AddJwtBearerOptions([NotNull] this AuthenticationBuilder thisValue, bool useAsDefault, Action<JwtBearerOptions> configureOptions = null)
		{
			return useAsDefault
				? thisValue.AddJwtBearer(configureOptions)
				: thisValue.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions);
		}
	}
}