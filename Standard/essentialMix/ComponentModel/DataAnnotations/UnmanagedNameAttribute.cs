using System;
using ATCommon.Extensions;
using JetBrains.Annotations;

namespace ATCommon.ComponentModel.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
	public class UnmanagedNameAttribute : Attribute
	{
		private readonly string _name;

		public UnmanagedNameAttribute([NotNull] string name)
		{
			name = name.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
			_name = name;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
