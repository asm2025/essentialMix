using System;
using System.Web.Mvc;
using asm.Extensions;
using asm.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace asm.Web.Mvc.Extensions
{
	public static class EnumExtension
	{
		[NotNull]
		public static SelectListItem ToListItem<T>(this T thisValue, bool selected = false, bool? disabled = null)
			where T : struct, Enum, IComparable
		{
			disabled ??= thisValue.HasAttribute<T, DisabledAttribute>();
			return new SelectListItem
			{
				Value = thisValue.ToString(),
				Text = thisValue.GetDisplayName(),
				Selected = selected,
				Disabled = disabled.Value
			};
		}
	}
}