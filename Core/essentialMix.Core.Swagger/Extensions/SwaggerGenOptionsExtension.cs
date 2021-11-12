﻿using System;
using System.Collections.Generic;
using essentialMix.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions
{
	public static class SwaggerGenOptionsExtension
	{
		[NotNull]
		public static SwaggerGenOptions Setup([NotNull] this SwaggerGenOptions thisValue, [NotNull] IConfiguration configuration, IHostEnvironment environment = null)
		{
			string website = configuration.GetAnyValue<string>(null, "swagger:website", "website");
			OpenApiInfo info = new OpenApiInfo
			{
				Title = configuration.GetAnyValue(environment?.ApplicationName, "swagger:title", "title"),
				Description = configuration.GetAnyValue(string.Empty, "swagger:description", "description")?.Replace("\n", "<br />"),
				Version = configuration.GetValue("swagger:version", "v1"),
				Contact = new OpenApiContact
				{
					Name = configuration.GetAnyValue<string>(null, "swagger:company", "company"),
					Email = configuration.GetAnyValue<string>(null, "swagger:email", "email"),
					Url = website == null
							? null
							: UriHelper.ToUri(website)
				}
			};

			return Setup(thisValue, info);
		}

		[NotNull]
		public static SwaggerGenOptions Setup([NotNull] this SwaggerGenOptions thisValue, [NotNull] OpenApiInfo info)
		{
			thisValue.SwaggerDoc(info.Version, info);
			thisValue.IgnoreObsoleteActions();
			thisValue.IgnoreObsoleteProperties();
			thisValue.OrderActionsBy(doc => doc.ActionDescriptor.RouteValues["controller"]);
			return thisValue;
		}

		[NotNull]
		public static SwaggerGenOptions AddJwtBearerSecurity([NotNull] this SwaggerGenOptions thisValue, string description = null)
		{
			if (string.IsNullOrEmpty(description))
			{
				description = @$"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme.
Please enter into field the word: {JwtBearerDefaults.AuthenticationScheme}[space][YOUR JWT TOKEN].
Example: {JwtBearerDefaults.AuthenticationScheme} 12345abcdef";
			}

			thisValue.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
			{
				In = ParameterLocation.Header,
				Name = HeaderNames.Authorization,
				Type = SecuritySchemeType.ApiKey,
				Description = description
			});

			thisValue.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Scheme = SecuritySchemeType.OAuth2.ToString(),
						Name = JwtBearerDefaults.AuthenticationScheme,
						In = ParameterLocation.Header,
						Reference = new OpenApiReference
						{
							Id = JwtBearerDefaults.AuthenticationScheme,
							Type = ReferenceType.SecurityScheme
						}
					},
					new List<string>()
				}
			});

			return thisValue;
		}

		[NotNull]
		public static SwaggerGenOptions AddOpenIdConnectSecurity([NotNull] this SwaggerGenOptions thisValue, [NotNull] Uri authorizationUrl, [NotNull] Uri tokenUrl, IDictionary<string, string> scopes, string description = null)
		{
			if (string.IsNullOrEmpty(description)) description = $"OAuth2 using the {OpenIdConnectDefaults.AuthenticationScheme} scheme.";
			scopes ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{"api", "API - full access"}
			};
			thisValue.AddSecurityDefinition(OpenIdConnectDefaults.AuthenticationScheme, new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.OAuth2,
				Description = description,
				Flows = new OpenApiOAuthFlows
				{
					AuthorizationCode = new OpenApiOAuthFlow
					{
						AuthorizationUrl = authorizationUrl,
						TokenUrl = tokenUrl,
						Scopes = scopes
					}
				}
			});

			thisValue.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Scheme = SecuritySchemeType.OAuth2.ToString(),
						Name = OpenIdConnectDefaults.AuthenticationScheme,
						Reference = new OpenApiReference
						{
							Id = OpenIdConnectDefaults.AuthenticationScheme,
							Type = ReferenceType.SecurityScheme
						}
					},
					new List<string>()
				}
			});

			return thisValue;
		}
	}
}