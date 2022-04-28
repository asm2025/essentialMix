using System;
using System.Collections.Generic;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property)]
public class DataNamesAttribute : Attribute
{
	protected readonly ISet<string> ValueNamesInternal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public DataNamesAttribute([NotNull] params string[] valueNames)
	{
		foreach (string valueName in valueNames.SkipNullOrEmptyTrim())
			ValueNamesInternal.Add(valueName);
	}

	public ISet<string> ValueNames => ValueNamesInternal;
}