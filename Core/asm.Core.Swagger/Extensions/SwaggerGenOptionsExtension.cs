using System.Collections.Generic;
using asm.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace asm.Extensions
{
	public static class SwaggerGenOptionsExtension
	{
		[NotNull]
		public static SwaggerGenOptions Setup([NotNull] this SwaggerGenOptions thisValue, [NotNull] IConfiguration configuration, IHostEnvironment environment = null)
		{
			string website = configuration.GetValue<string>(null, "swagger:website", "website");

			OpenApiInfo info = new OpenApiInfo
			{
				Title = configuration.GetValue(environment?.ApplicationName, "swagger:title", "title"),
				Description = configuration.GetValue(string.Empty, "swagger:description", "description")?.Replace("\n", "<br />"),
				Version = configuration.GetValue("swagger:version", "v1"),
				Contact = new OpenApiContact
				{
					Name = configuration.GetValue<string>(null, "swagger:company", "company"),
					Email = configuration.GetValue<string>(null, "swagger:email", "email"),
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
				description = @"JWT Authorization header using the Bearer scheme.
Please enter into field the word: Bearer[space][YOUR JWT TOKEN].
Example: Bearer 12345abcdef";
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
	}
}