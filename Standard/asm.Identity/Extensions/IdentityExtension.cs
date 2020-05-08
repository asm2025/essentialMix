using System.Collections.Generic;
using System.Linq;
using System.Text;
using asm.Extensions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace asm.Identity.Extensions
{
	public static class IdentityExtension
	{
		public static string CollectMessages([NotNull] this IEnumerable<IdentityError> thisValue, string message = null)
		{
			return thisValue.Aggregate(new StringBuilder(message ?? string.Empty), (builder, error) => builder.AppendWithLine($"[{error.Code}] {error.Description}"), builder => builder.ToString());
		}
	}
}