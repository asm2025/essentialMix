using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class OperationFilterContextExtension
{
	[NotNull]
	public static IEnumerable<T> GetControllerAndActionAttributes<T>([NotNull] this OperationFilterContext thisValue)
		where T : Attribute
	{
		return thisValue.MethodInfo.GetMemberAndTypeAttributes<T>();
	}
}