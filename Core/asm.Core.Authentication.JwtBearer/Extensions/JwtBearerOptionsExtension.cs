using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class JwtBearerOptionsExtension
	{
		[NotNull]
		public static JwtBearerOptions Setup([NotNull] this JwtBearerOptions thisValue, [NotNull] SecurityKey signingKey, [NotNull] IConfiguration configuration, IHostEnvironment environment = null)
		{
			return Setup(thisValue, signingKey, null, configuration, environment?.IsDevelopment() == true);
		}

		[NotNull]
		public static JwtBearerOptions Setup([NotNull] this JwtBearerOptions thisValue, [NotNull] SecurityKey signingKey, SecurityKey decryptionKey, [NotNull] IConfiguration configuration, IHostEnvironment environment = null)
		{
			return Setup(thisValue, signingKey, decryptionKey, configuration, environment?.IsDevelopment() == true);
		}

		[NotNull]
		public static JwtBearerOptions Setup([NotNull] this JwtBearerOptions thisValue, [NotNull] SecurityKey signingKey, [NotNull] IConfiguration configuration, bool isDevelopment = false)
		{
			return Setup(thisValue, signingKey, null, configuration, isDevelopment);
		}

		[NotNull]
		public static JwtBearerOptions Setup([NotNull] this JwtBearerOptions thisValue, [NotNull] SecurityKey signingKey, SecurityKey decryptionKey, [NotNull] IConfiguration configuration, bool isDevelopment = false)
		{
			if (isDevelopment)
			{
				thisValue.RequireHttpsMetadata = false;
				thisValue.IncludeErrorDetails = true;
			}

			IList<string> validIssuers = configuration.GetValue<string>("jwt:issuer").Split(StringSplitOptions.RemoveEmptyEntries, ',');
			if (validIssuers.Count == 0) validIssuers = null;
			
			IList<string> validAudiences = configuration.GetValue<string>("jwt:audience").Split(StringSplitOptions.RemoveEmptyEntries, ',');
			if (validAudiences.Count == 0) validAudiences = null;

			thisValue.SaveToken = true;
			thisValue.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				ValidateLifetime = true,
				IssuerSigningKey = signingKey,
				TokenDecryptionKey = decryptionKey,
				ValidIssuers = validIssuers,
				ValidAudiences = validAudiences,
				ClockSkew = TimeSpan.Zero,
				ValidateIssuer = validIssuers != null,
				ValidateAudience = validAudiences != null
			};

			return thisValue;
		}
	}
}