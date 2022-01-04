using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using essentialMix.Helpers;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DataEntityExceptionExtension
{
	[NotNull]
	public static string CollectDataMessages(this Exception thisValue)
	{
		if (thisValue == null) return string.Empty;

		StringBuilder sb = new StringBuilder();
		ExceptionHelper.CollectMessages(thisValue, sb, exception =>
		{
			if (exception is not DbEntityValidationException ve) return;

			foreach (DbEntityValidationResult result in ve.EntityValidationErrors.Where(ex => !ex.IsValid))
			{
				foreach (DbValidationError error in result.ValidationErrors)
					sb.AppendWithLine($"{error.PropertyName}: {error.ErrorMessage}");
			}
		});
		return sb.ToString();
	}
}