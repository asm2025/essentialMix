using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class MemberInfoExtension
{
	public static bool HasUnderlyingType([NotNull] this MemberInfo thisValue)
	{
		switch (thisValue.MemberType)
		{
			case MemberTypes.Event:
			case MemberTypes.Field:
			case MemberTypes.Method:
			case MemberTypes.Property:
				return true;
			default:
				return false;
		}
	}

	public static Type GetUnderlyingType([NotNull] this MemberInfo thisValue)
	{
		switch (thisValue.MemberType)
		{
			case MemberTypes.Event:
				return ((EventInfo)thisValue).EventHandlerType;
			case MemberTypes.Field:
				return ((FieldInfo)thisValue).FieldType;
			case MemberTypes.Method:
				return ((MethodInfo)thisValue).ReturnType;
			case MemberTypes.Property:
				return ((PropertyInfo)thisValue).PropertyType;
			default:
				return null;
		}
	}

	public static bool IsDefined<T>([NotNull] this MemberInfo thisValue, bool inherit = true)
		where T : Attribute
	{
		return thisValue.IsDefined(typeof(T), inherit);
	}

	public static bool IsBrowsable([NotNull] this MemberInfo thisValue)
	{
		BrowsableAttribute browsable = thisValue.GetAttribute<BrowsableAttribute>(true);
		return browsable is not { Browsable: not true };
	}

	[NotNull]
	public static IEnumerable<T> GetMemberAndTypeAttributes<T>([NotNull] this MemberInfo thisValue)
		where T : Attribute
	{
		IEnumerable<T> memberAttributes = thisValue.DeclaringType.GetTypeInfo().GetCustomAttributes<T>();
		IEnumerable<T> actionAttributes = thisValue.GetCustomAttributes<T>();
		return memberAttributes.Union(actionAttributes);
	}
}