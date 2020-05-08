using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using asm.Core.Data.Entity.Exceptions;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Core.Data.Entity.Extensions
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

				foreach (ValidationResult result in ve.EntityValidationErrors)
				{
					sb.AppendWithLine($@"The following member(s): {string.Join(", ", result.MemberNames)}
has this validation error: {result.ErrorMessage}");
				}
			});
			return sb.ToString();
		}
	}
}