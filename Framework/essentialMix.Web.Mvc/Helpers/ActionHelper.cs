using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Web.Mvc.Helpers;

public static class ActionHelper
{
	[NotNull]
	public static IEnumerable<MethodInfo> GetAllActions()
	{
		return IControllerHelper.GetAllControllers()
								.SelectMany(e => e.GetMethods(Constants.BF_PUBLIC_INSTANCE | BindingFlags.DeclaredOnly))
								.Where(e => !e.HasAttribute<CompilerGeneratedAttribute>());
	}
}