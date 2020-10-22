using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using asm.Extensions;
using asm.Web;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace asm.Core.Swagger.Filters
{
	public class FormFileFilter : IOperationFilter
	{
		/// <inheritdoc />
		public void Apply([NotNull] OpenApiOperation operation, [NotNull] OperationFilterContext context)
		{
			if (operation.Deprecated
				|| !Enum.TryParse(context.ApiDescription.HttpMethod, true, out HttpMethod method)
				|| !method.In(HttpMethod.Post, HttpMethod.Put))
			{
				return;
			}

			// Check if any of the parameters' types or their nested properties / fields are supported
			if (!Enumerate(context.ApiDescription.ActionDescriptor).Any(e => IsSupported(e.ParameterType))) return;

			OpenApiMediaType uploadFileMediaType = operation.RequestBody.Content.GetOrAdd(MediaTypeNames.Multipart.FormData, e => new OpenApiMediaType
			{
				Schema = new OpenApiSchema
				{
					Type = "object",
					Properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase),
					Required = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				}
			}); 

			IDictionary<string, OpenApiSchema> schemaProperties = uploadFileMediaType.Schema.Properties;
			ISet<string> schemaRequired = uploadFileMediaType.Schema.Required;
			ISchemaGenerator generator = context.SchemaGenerator;
			SchemaRepository repository = context.SchemaRepository;
			
			foreach (ParameterDescriptor parameter in Enumerate(context.ApiDescription.ActionDescriptor))
			{
				OpenApiSchema schema = generator.GenerateSchema(parameter.ParameterType, repository);
				if (schema == null) continue;

				if (IsSupported(parameter.ParameterType))
				{
					schema.Type = "file";
				}
				schemaProperties.Add(parameter.Name, schema);
				
				if (parameter.ParameterType.IsPrimitive && !parameter.ParameterType.IsNullable()
					|| !parameter.ParameterType.IsInterface && !parameter.ParameterType.IsClass || parameter.ParameterType.HasAttribute<RequiredAttribute>())
				{
					schemaRequired.Add(parameter.Name);
				}
			}

			static IEnumerable<ParameterDescriptor> Enumerate(ActionDescriptor descriptor)
			{
				foreach (ParameterDescriptor parameter in descriptor.Parameters.Where(p => p.BindingInfo.BindingSource.IsFromRequest && !p.BindingInfo.BindingSource.Id.IsSame("Path")))
				{
					yield return parameter;
				}
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		private static bool IsSupported(Type type)
		{
			return type != null && (type.IsAssignableFrom(typeof(IFormFile)) || type.IsAssignableFrom(typeof(IFormFileCollection)));
		}
	}
}