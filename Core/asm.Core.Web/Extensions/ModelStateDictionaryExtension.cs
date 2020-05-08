using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace asm.Core.Web.Extensions
{
	public static class ModelStateDictionaryExtension
	{
		[NotNull]
		public static string CollectMessages(this ModelStateDictionary modelState)
		{
			if (modelState == null || modelState.IsValid) return string.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (ModelStateEntry state in modelState.Values.Where(v => v.Errors.Count > 0))
			{
				foreach (ModelError stateError in state.Errors)
					sb.AppendLine(stateError.ErrorMessage);
			}

			return sb.ToString();
		}
	}
}