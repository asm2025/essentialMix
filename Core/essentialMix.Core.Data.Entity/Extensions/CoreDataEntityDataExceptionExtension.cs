using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using essentialMix.Core.Data.Entity.Exceptions;
using essentialMix.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class CoreDataEntityDataExceptionExtension
{
	[NotNull]
	public static string CollectDataMessages(this Exception thisValue)
	{
		if (thisValue == null) return string.Empty;

		StringBuilder sb = new StringBuilder();
		ExceptionHelper.CollectMessages(thisValue, sb, exception =>
		{
			if (exception is not DbEntityValidationException ve) return;

			foreach (ValidationResult result in ve.EntityValidationErrors)
			{
				sb.AppendWithLine($@"The following member(s): {string.Join(", ", result.MemberNames)}
has this validation error: {result.ErrorMessage}");
			}
		});
		return sb.ToString();
	}
}