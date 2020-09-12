using System;
using System.Collections.Generic;
using asm.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DataNamesAttribute : Attribute
	{
		protected readonly ISet<string> ValueNamesInternal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public DataNamesAttribute([NotNull] params string[] valueNames)
		{
			foreach (string valueName in valueNames.SkipNullOrEmptyTrim()) 
				ValueNamesInternal.Add(valueName);

			ValueNames = new ReadOnlySet<string>(ValueNamesInternal);
		}

		public IReadOnlySet<string> ValueNames { get; }
	}
}