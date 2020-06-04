using System;
using System.Management;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class ManagementBaseObjectExtension
	{
		private const string KEY_DEF = "Name";

		public static object PropertyOrSelf([NotNull] this ManagementBaseObject thisValue, [NotNull] string key = KEY_DEF)
		{
			if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

			try
			{
				return thisValue[key];
			}
			catch
			{
				return thisValue;
			}
		}

		[NotNull]
		public static string PropertyOrString([NotNull] this ManagementBaseObject thisValue, [NotNull] string key = KEY_DEF) { return Convert.ToString(PropertyOrSelf(thisValue, key)); }
	}
}