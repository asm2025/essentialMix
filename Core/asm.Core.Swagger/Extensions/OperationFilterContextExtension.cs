using System;
using System.Collections.Generic;
using asm.Extensions;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace asm.Core.Swagger.Extensions
{
	public static class OperationFilterContextExtension
	{
		[NotNull]
		public static IEnumerable<T> GetControllerAndActionAttributes<T>([NotNull] this OperationFilterContext thisValue)
			where T : Attribute
		{
			return thisValue.MethodInfo.GetMemberAndTypeAttributes<T>();
		}
	}
}