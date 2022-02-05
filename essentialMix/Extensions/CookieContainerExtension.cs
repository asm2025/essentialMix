using System.Collections;
using System.Net;
using System.Reflection;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class CookieContainerExtension
{
	public static CookieCollection Cookies([NotNull] this CookieContainer thisValue)
	{
		if (thisValue.Count == 0) return null;

		CookieCollection collection = new CookieCollection();
		FieldInfo domainTableField = thisValue.GetType().GetField("m_domainTable", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.GetField);
		if (domainTableField == null) return collection;

		IDictionary domains = (IDictionary)domainTableField.GetValue(thisValue);

		foreach (object val in domains.Values)
		{
			FieldInfo type = val.GetType().GetField("m_list", Constants.BF_NON_PUBLIC_INSTANCE | BindingFlags.GetField);
			if (type == null) continue;

			IDictionary values = (IDictionary)type.GetValue(val);

			foreach (CookieCollection cookies in values.Values)
			{
				collection.Add(cookies);
			}
		}

		return collection;
	}
}