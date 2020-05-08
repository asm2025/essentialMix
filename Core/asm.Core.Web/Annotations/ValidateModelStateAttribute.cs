using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace asm.Core.Web.Annotations
{
	public class ValidateModelStateAttribute : ActionFilterAttribute
	{
		/// <inheritdoc />
		public override void OnActionExecuting([NotNull] ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState);
				return;
			}

			base.OnActionExecuting(context);
		}
	}
}
