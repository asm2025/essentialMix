using System;
using System.Collections.Generic;
using asm.Extensions;
using asm.Collections;
using JetBrains.Annotations;

namespace asm.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DataNamesAttribute : Attribute
	{
		protected readonly ISet<string> ValueNamesInternal = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public DataNamesAttribute([NotNull] params string[] valueNames)
		{
			ValueNamesInternal.AddRange(valueNames.SkipNullOrEmptyTrim());
			ValueNames = new ReadOnlySet<string>(ValueNamesInternal);
		}

		public IReadOnlySet<string> ValueNames { get; }
	}
}