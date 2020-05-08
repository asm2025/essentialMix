using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace asm.Core.Authentication.JwtBearer.Extensions
{
	public static class AuthenticationBuilderExtension
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