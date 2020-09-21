using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class JwtBearerIServiceCollectionExtension
	{
		public static AuthenticationBuilder AddJwtBearerAuthentication([NotNull] this IServiceCollection thisValue, Action<AuthenticationOptions> configureOptions = null)
		{
			return thisValue.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
				configureOptions?.Invoke(options);
			});
		}
	}
}