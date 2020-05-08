using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Data.Entity.Extensions
{
	public static class ExceptionExtension
	{
		[NotNull]
		public static string CollectDataMessages(this Exception thisValue)
		{
			if (thisValue == null) return string.Empty;

			StringBuilder sb = new StringBuilder();
			ExceptionHelper.CollectMessages(thisValue, sb, exception =>
			{
				if (!(exception is DbEntityValidationException ve)) return;

				foreach (DbEntityValidationResult result in ve.EntityValidationErrors.Where(ex => !ex.IsValid))
				{
					foreach (DbValidationError error in result.ValidationErrors)
						sb.AppendWithLine($"{error.PropertyName}: {error.ErrorMessage}");
				}
			});
			return sb.ToString();
		}
	}
}