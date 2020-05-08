using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace asm.Core.Swagger.Extensions
{
	public static class SwaggerUIOptionsExtension
	{
		[NotNull]
		public static SwaggerUIOptions AsStartPage([NotNull] this SwaggerUIOptions thisValue)
		{
			thisValue.RoutePrefix = string.Empty;
			return thisValue;
		}
	}
}