using System.Collections;
using System.Text;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class DictionaryEntryExtension
	{
		[NotNull]
		public static string Format(this DictionaryEntry thisValue, string format, char separator = '=')
		{
			return !HasKeyAndValue(thisValue)
						? string.Empty
						: $"{thisValue.Key}{separator}{FormatValue(format, thisValue.Value)}";
		}

		[NotNull]
		public static string Format(this DictionaryEntry thisValue, string format, string separator)
		{
			return !HasKeyAndValue(thisValue)
						? string.Empty
						: string.Concat(thisValue.Key, separator, FormatValue(format, thisValue.Value));
		}

		[NotNull]
		public static string ToString(this DictionaryEntry thisValue, char separator) { return Format(thisValue, "{0}", separator); }

		[NotNull]
		public static string ToString(this DictionaryEntry thisValue, string separator) { return Format(thisValue, "{0}", separator); }

		public static bool HasKey(this DictionaryEntry thisValue) { return thisValue.Key != null; }

		public static bool HasValue(this DictionaryEntry thisValue) { return thisValue.Value != null; }

		public static bool HasKeyAndValue(this DictionaryEntry thisValue) { return thisValue.Key != null && thisValue.Value != null; }

		[NotNull]
		private static string FormatValue<TValue>(string format, TValue value)
		{
			if (string.IsNullOrEmpty(format) || value.IsNull()) return string.Empty;

			if (!(value is IEnumerable enumerable)) return string.Format(format, value);

			StringBuilder sb = new StringBuilder();

			foreach (object item in enumerable)
			{
				if (item.IsNull()) continue;
				sb.Separator(',');
				sb.AppendFormat(format, item);
			}

			return sb.ToString();
		}
	}
}