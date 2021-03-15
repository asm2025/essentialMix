using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions
{
	public static class PropertyInfoExtension
	{
		public static int StringPropertyLength([NotNull] this PropertyInfo thisValue)
		{
			Type type = thisValue.PropertyType.ResolveType();
			if (!type.Is<string>()) throw new ArgumentException("Property is not a string.", nameof(thisValue));

			int len = 0;
			IEnumerable<ValidationAttribute> attributes = thisValue.GetAttributes<ValidationAttribute>(true)
				.Where(e => e.Is(typeof(StringLengthAttribute)) || e.Is(typeof(MaxLengthAttribute)) || e.Is(typeof(MinLengthAttribute)));

			foreach (ValidationAttribute attribute in attributes)
			{
				switch (attribute)
				{
					case StringLengthAttribute sl:
						if (len < sl.MaximumLength) len = sl.MaximumLength;
						break;
					case MaxLengthAttribute ml:
						if (len < ml.Length) len = ml.Length;
						break;
					case MinLengthAttribute ml:
						if (len < ml.Length) len = ml.Length;
						break;
				}
			}

			return len;
		}

		public static bool HasArguments([NotNull] this PropertyInfo thisValue) { return thisValue.GetIndexParameters().Length > 0; }
	}
}