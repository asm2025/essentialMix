using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class PropertyInfoExtension
{
	public static bool HasArguments([NotNull] this PropertyInfo thisValue) { return thisValue.GetIndexParameters().Length > 0; }
}