using System.Linq;
using System.Text;
using System.Web.WebPages.Html;
using JetBrains.Annotations;

namespace asm.Web.Pages.Extensions
{
	public static class ModelStateDictionaryExtension
	{
		[NotNull]
		public static string CollectMessages(this ModelStateDictionary modelState)
		{
			if (modelState == null || modelState.IsValid) return string.Empty;

			StringBuilder sb = new StringBuilder();

			foreach (ModelState state in modelState.Values.Where(v => v.Errors.Count > 0))
			{
				foreach (string error in state.Errors)
					sb.AppendLine(error);
			}

			return sb.ToString();
		}
	}
}