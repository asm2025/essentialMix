using System.Globalization;
using System.IO;
using System.Resources;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class ResourceManagerExtension
	{
		public static bool TryGetString([NotNull] this ResourceManager thisValue, [NotNull] string name, out string value)
		{
			return TryGetString(thisValue, name, null, out value);
		}

		public static bool TryGetString([NotNull] this ResourceManager thisValue, [NotNull] string name, CultureInfo cultureInfo, out string value)
		{
			try
			{
				value = thisValue.GetString(name, cultureInfo);
			}
			catch
			{
				value = null;
			}

			return value != null;
		}

		public static bool TryGetObject([NotNull] this ResourceManager thisValue, [NotNull] string name, out object value)
		{
			return TryGetObject(thisValue, name, null, out value);
		}

		public static bool TryGetObject([NotNull] this ResourceManager thisValue, [NotNull] string name, CultureInfo cultureInfo, out object value)
		{
			try
			{
				value = thisValue.GetObject(name, cultureInfo);
			}
			catch
			{
				value = null;
			}

			return value != null;
		}

		public static bool TryGetStream([NotNull] this ResourceManager thisValue, [NotNull] string name, out UnmanagedMemoryStream value)
		{
			return TryGetStream(thisValue, name, null, out value);
		}

		public static bool TryGetStream([NotNull] this ResourceManager thisValue, [NotNull] string name, CultureInfo cultureInfo, out UnmanagedMemoryStream value)
		{
			try
			{
				value = thisValue.GetStream(name, cultureInfo);
			}
			catch
			{
				value = null;
			}

			return value != null;
		}
	}
}