using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using JetBrains.Annotations;
using asm.Data.Patterns.Table;
using asm.Extensions;

namespace asm.Data.Helpers
{
	public static class TableColumnHelper
	{
		public static TableColumnSettings? PrepareAndFilter([NotNull] string name, [NotNull] IReadOnlyDictionary<string, (string Name, TableColumnSettings Settings)> dictionary, [NotNull] ResourceManager resourceManager, CultureInfo culture)
		{
			string resourceName;
			TableColumnSettings settings;

			if (!dictionary.TryGetValue(name, out (string Name, TableColumnSettings Settings) tuple))
			{
				resourceName = name;
				settings = new TableColumnSettings();
			}
			else
			{
				resourceName = tuple.Name ?? name;
				settings = tuple.Settings;
			}

			if (resourceManager.TryGetString(resourceName, culture, out string text))
			{
				settings.Text = text;
			}
			else
			{
				settings.Text = resourceName;
				settings.Hidden = true;
			}

			return settings;
		}
	}
}
