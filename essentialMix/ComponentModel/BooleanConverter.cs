using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Threading;

namespace essentialMix.ComponentModel;

/// <inheritdoc />
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class BooleanConverter : System.ComponentModel.BooleanConverter
{
	private static volatile StandardValuesCollection __values;
	private static volatile ISet<string> __trueValues;
	private static volatile ISet<string> __falseValues;

	/// <inheritdoc />
	public BooleanConverter()
	{
	}

	/// <inheritdoc />
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is not string str) return base.ConvertFrom(context, culture, value);
		str = str.Trim();
		if (bool.TryParse(str, out bool result)) return result;
		if (GetTrueValues().Contains(str)) return true;
		if (GetFalseValues().Contains(str)) return false;
		throw new FormatException($"String '{str}' was not recognized as a valid Boolean.");
	}

	/// <inheritdoc />
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		Thread.MemoryBarrier();
		if (__values == null) Interlocked.CompareExchange(ref __values, new StandardValuesCollection(new List<string>(GetTrueValues().Concat(GetFalseValues()))), null);
		return base.GetStandardValues(context);
	}

	protected ISet<string> GetTrueValues()
	{
		if (__trueValues == null)
		{
			Interlocked.CompareExchange(ref __trueValues, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"true",
				"yes",
				"on",
				"enable",
				"enabled",
				"allow",
				"allowed"
			}, null);
		}

		return __trueValues;
	}

	protected ISet<string> GetFalseValues()
	{
		if (__falseValues == null)
		{
			Interlocked.CompareExchange(ref __falseValues, new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"false",
				"no",
				"off",
				"disable",
				"disabled",
				"disallow",
				"disallowed"
			}, null);
		}

		return __falseValues;
	}
}