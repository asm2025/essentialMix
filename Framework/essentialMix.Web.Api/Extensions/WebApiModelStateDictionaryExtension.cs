using System.Linq;
using System.Text;
using System.Web.Http.ModelBinding;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class WebApiModelStateDictionaryExtension
{
	[NotNull]
	public static string CollectMessages(this ModelStateDictionary modelState)
	{
		if (modelState == null || modelState.IsValid) return string.Empty;

		StringBuilder sb = new StringBuilder();

		foreach (ModelState state in modelState.Values.Where(v => v.Errors.Count > 0))
		{
			foreach (ModelError stateError in state.Errors)
				sb.AppendLine(stateError.ErrorMessage);
		}

		return sb.ToString();
	}
}